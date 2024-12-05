package Server;

import Network.Tcp.TcpServerController;
import Network.UdpHandler;
import Utility.WordGenerator;
import Utility.SystemUtility.*;
import java.time.Instant;
import java.util.List;
import java.util.concurrent.Semaphore;

/**
 * Server for the drawing guessing game.
 * This server is multithreaded and hosts a game for any number of clients.
 * It uses TCP for communicating the game and client state, and uni-cast UDP for relaying drawing data.
 */
public class DrawServer {
    private static final int TCP_PORT = 5000;  // Port for TCP connections
    private static final int UDP_PORT = 5001;  // Port for UDP connections
    private static String secretWord = null; // The word to paint/guess
    private static Client currentPainter; // The currently selected painter
    private static volatile boolean startNewRound = true; // Flag for when to start a new round

    // The start-time of the current round (only valid if startNewRound == false)
    private static long startNewRoundTime;

    /* Ensures that clients can't be removed or guess while a new round is being set up.
    * Note: Fair = true prevents thread starvation.*/
    private static final Semaphore newRoundSemaphore = new Semaphore(1,true);


    /**
     * Interrupt an ongoing round
     * @param message An explanation for why the round was interrupted.
     */
    private static void interruptRound(String message){
        // Broadcast interruption
        ClientManager.broadcastSystemMessage(CommandType.INTERRUPT_ROUND, message);
        // Enable start of round
        endRound();
    }

    /**
     * Stops an active round and sets the server up for a new round of gameplay.
     */
    private static void endRound(){
        secretWord = null; // Disable further guessing
        startNewRound = true; // Enable start of round
        // Broadcast that the round has ended
        ClientManager.broadcastSystemMessage(CommandType.END_ROUND);
    }

    /**
     * Check if the id belongs to the current painter.
     * @param id The compared id.
     * @return True if the id belongs to the current painter; otherwise, false.
     */
    public static boolean isCurrentPainter(int id){
        return currentPainter != null && id == currentPainter.getId();
    }


    /**
     * Get elapsed time since the start time of the current round.
     * @return The elapsed time in seconds.
     */
    private static int getElapsedRoundTime(){
        return (int) (Instant.now().toEpochMilli() - startNewRoundTime) / 1000;
    }

    /**
     * Get remaining time of the current round.
     * @return The remaining time in seconds.
     */
    private static int getRemainingRoundTime(){
        // Calculate remaining time
        int remainingTime = Utility.SystemUtility.ROUND_LENGTH - (int) (Instant.now().toEpochMilli() - startNewRoundTime) / 1000;
        // if a round is ongoing or there is any remaining time left
        if(!startNewRound && remainingTime > 0){
            // return the remaining time in seconds
            return remainingTime;
        }
        return 0;
    }

    /**
     * Attempt to start a new round and inform all clients.
     * @return True if the round could start successfully; otherwise, null.
     */
    private static boolean tryStartNewRound(){
        try {
            // Ensures that clients can't be removed or guess while a new round is being set up.
            newRoundSemaphore.acquire();
            // Try to find a new painter
            Client nextPainter = ClientManager.tryGetNextPainter();
            if (nextPainter != null) {
                // New painter was found
                currentPainter = nextPainter;
                // Announce that a new round is about to start
                ClientManager.broadcastSystemMessage(CommandType.NEW_ROUND);
                // Update currentWordToGuess
                secretWord = WordGenerator.getNextWord();
                // Tell the new painter about the secret word
                currentPainter.addTcpSystemMessage(CommandType.SECRET_WORD, List.of(secretWord));
                // Broadcast that a new round has started
                ClientManager.broadcastSystemMessage(CommandType.START_ROUND, currentPainter.getUsername());
                // Save current time
                startNewRoundTime = Instant.now().toEpochMilli();
                return true;
            }
        } catch (InterruptedException e) {
            System.out.println("Main thread was interrupted: " + e.getMessage());
        } finally {
            newRoundSemaphore.release();
        }
        return false;
    }

    /**
     * Initializes TCP and UDP reader/writer threads, and continuously attempts to start new rounds of gameplay.
     * @param args Ignored
     */
    public static void main(String[] args) {
        // Start TCP server controller
        Thread tcpThread = new Thread(new TcpServerController(TCP_PORT));
        tcpThread.start();

        // Start UDP handler
        Thread udpThread = new Thread(new UdpHandler(UDP_PORT));
        udpThread.start();

        // Start new rounds while server is alive
        while (true) {
                // If a round is currently active, check if the round has exceeded the round time
                if (!startNewRound && getElapsedRoundTime() > Utility.SystemUtility.ROUND_LENGTH) {
                    endRound();
                }
                // Check if a new round should start
                if (startNewRound) {
                    ClientManager.broadcastMessage("Finding new painter...");
                    // Try to start a new round until the operation is successful
                    while (startNewRound){
                        if(tryStartNewRound()){
                            // Reset the new round flag
                            startNewRound = false;
                        }
                    }
                }
            try {
                // Have the thread sleep during rounds to not unnecessarily use up system resources
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                System.out.println("Main thread was interrupted while sleeping:" + e.getMessage());
            }
        }
    }

    /**
     * Verify if the guess matches the current word to be guessed.
     * @param guess The guessed word.
     * @return True if the user guessed the correct word first; otherwise false.
     */
    public static boolean VerifyGuess(String guess, Client client){
        try {
            /*Ensures that clients can't guess while a new round is being set up.
            If a new round has already been set up while client waited to guess the secret
            word of the previous round, then the guess will likely be incorrect this round.*/
            newRoundSemaphore.acquire();
            // Check if the guess is correct
            if (secretWord != null && guess != null && secretWord.equals(guess.toLowerCase())) {
                // Disable any further guessing until a new round is set up
                secretWord = null;
                // Calculate remaining time of round
                int remainingTimeInSeconds = Utility.SystemUtility.ROUND_LENGTH - getElapsedRoundTime();
                // Give guesser 10 times the amount of seconds in points
                int rewardedPoints = remainingTimeInSeconds * 10;
                client.addPoints(rewardedPoints);
                // Enable start of new round
                endRound();
                return true;
            }
        } catch (InterruptedException e) {
            System.out.println("Interrupted when acquiring semaphore in VerifyGuess: " + e.getMessage());
        } finally {
            newRoundSemaphore.release();
        }
        return false;
    }

    /**
     * Add a new client. If there is an active round, prompt them to join.
     * @param client The client to be added
     */
    public static void addClient(Client client){
        ClientManager.addClient(client); // Add client to list
        // Check if a round is currently active
        if(!startNewRound && getRemainingRoundTime() > 0){
            // Let the client join the current round
            client.addTcpSystemMessage(CommandType.JOIN_ROUND, List.of(currentPainter.getUsername(),
                    String.valueOf(getRemainingRoundTime())));
        }
    }

    /**
     * Wait to remove a client from the clients list.
     * This might interrupt a round if the client to be removed is the current painter.
     * @param client The client to be removed.
     */
    public static void removeClient(Client client){
        try {
            /* Ensures that a client isn't removed while a new round is starting.
            If a new round has already been set up while client is currentPainter before
            the semaphore is acquired, then the client will not be the same as currentPainter
            anymore. If client turned into currentPainter during this, then we should start a new round.*/
            newRoundSemaphore.acquire();

            // Remove client
            ClientManager.removeClient(client);
            if (client.equals(currentPainter)) {
                // removed player was the current painter
                interruptRound("The round was interrupted because the painter disconnected.");
            }else if(currentPainter != null && ClientManager.numOfClients() < 2){
                // There's not enough players left to play the game
                interruptRound("The round was interrupted because there are too few players.");
            }

        } catch (InterruptedException e) {
            System.out.println("Interrupted when acquiring semaphore in removeClient: " + e.getMessage());
        } finally {
            newRoundSemaphore.release();
        }
    }
}
