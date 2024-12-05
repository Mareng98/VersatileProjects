package Client;

import Interface.Gui;
import Interface.Paper;
import Network.DrawingListener;
import Network.DrawingSender;
import Network.MessageReader;
import Network.MessageWriter;
import Utility.SystemUtility.*;

import java.awt.*;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.*;
import java.net.DatagramSocket;
import java.util.List;

/**
 * Client for the drawing guessing game.
 * This client is multithreaded and joins game sessions through the server.
 * Any number of clients can join the same game.
 */
public class DrawClient {
    private static final Paper paper = new Paper(); // The drawing context
    private static final int TCP_PORT = 5000; // Server's TCP_PORT
    private static final int UDP_PORT = 5001; // Server's UDP_PORT
    private static volatile boolean isCurrentPainter = false; // If the client should paint or not
    private static String secretWord = null; // The word to paint if isCurrentPainter = true; Otherwise, invalid
    private static int id; // ID of client
    private static volatile boolean serverConnectionClosed = false; // Flag for if the server stops responding

    /**
     * Initializes the client, and then waits for program to close.
     * @param args The username followed by the address of the server.
     */
    public static void main(String[] args) {
        // Initialize variables
        String username;
        String serverAddress;
        try{
            // Get username from args
            username = getUsername(args);
            // Get server address from args
            serverAddress = getServerAddress(args);
        }catch (IllegalArgumentException e){
            // The arguments were invalid, exit the program
            System.out.println(e.getMessage());
            return;
        }

        // Create GUI
        Gui gui = new Gui(paper);
        try (Socket tcpSocket = new Socket(serverAddress, TCP_PORT)) {
            PrintWriter tcpOut = new PrintWriter(tcpSocket.getOutputStream(), true);
            BufferedReader tcpIn = new BufferedReader(new InputStreamReader(tcpSocket.getInputStream()));

            // Create UDP socket with any available port number
            try (DatagramSocket udpSocket = new DatagramSocket()) {

                // Start the TCP reader thread
                MessageReader tcpReader = new MessageReader(tcpIn);
                tcpReader.start();

                // Start the TCP writer thread
                MessageWriter tcpWriter = new MessageWriter(tcpOut);
                tcpWriter.start();

                // Send username and the UDP port number to use to the server
                List<String> userArgs = List.of(username, String.valueOf(udpSocket.getLocalPort()));
                Command userArgsCommand = new Command(CommandType.CLIENT_ARGS,
                        userArgs);
                tcpOut.println(userArgsCommand);

                // Start the UDP drawing sender that sends new points drawn by the client to the server
                DrawingSender drawingSender = new DrawingSender(UDP_PORT, serverAddress, udpSocket);

                // Start the UDP drawing listener that listens for new points received from the server
                DrawingListener drawingListener = new DrawingListener(udpSocket);

                // Keep the main thread alive as long as the server is alive
                while (!serverConnectionClosed) {
                    Thread.sleep(1000);
                }
                // Kill the TCP and UDP handler threads to close the program
                tcpReader.kill();
                tcpWriter.kill();
                tcpReader.join();
                tcpWriter.join();

                drawingSender.kill();
                drawingListener.kill();
                drawingSender.join();
                drawingListener.join();

            } catch (InterruptedException e) {
                System.err.printf("Could not connect to %s\n%s", serverAddress, e.getMessage());
            }
        } catch (IOException e) {
            System.err.printf("Could not connect to %s\n%s", serverAddress, e.getMessage());
        } catch (IllegalArgumentException e){
            System.out.println(e.getMessage());
        }
        // Inform the client that the program will be closed
        try {
            for(int i = 5; i > 0; i--){
                Gui.setHeaderLabel("Server Disconnected! Shutting the program down in "+ i + " seconds.");
                Thread.sleep(1000);
            }

        }catch (InterruptedException e){
            System.out.println(e.getMessage());
        }
        gui.dispose();
        System.out.println("\nExiting...");
    }

    /**
     * Get the username from the program arguments.
     * @param args Username follow by the server's address.
     * @return The username.
     * @throws IllegalArgumentException If the username wasn't provided.
     */
    public static String getUsername(String[] args) throws IllegalArgumentException{
        if(args.length >= 1){
            return args[0];

        }else{
            throw new IllegalArgumentException("A username wasn't provided! Exiting the program.");
        }
    }

    /**
     * Get the server address from the program arguments.
     * @param args Username follow by the server's address.
     * @return The server's InetAddress.
     * @throws IllegalArgumentException If the server address wasn't provided.
     */
    public static String getServerAddress(String[] args) throws IllegalArgumentException{
        if(args.length >= 2){
            return args[1];
        }else{
            throw new IllegalArgumentException("A server address wasn't provided! Exiting the program.");
        }
    }

    /**
     * Start exiting the program.
     */
    public static void setServerConnectionClosed(){
        serverConnectionClosed = true;
    }

    /**
     * Adds an incoming point to the canvas
     * @param p The point to be drawn
     */
    public static void addPointToDraw(Point p) {
        paper.addReceivedPoint(p);
    }

    /**
     * Clears the drawing canvas.
     */
    public static void clearPaper(){
        paper.clear();
    }

    /**
     * Get the secret word to be drawn
     * @return The secret word.
     */
    public static String getSecretWord(){
        return secretWord;
    }

    /**
     * Get the ID of the client.
     * @return The ID.
     */
    public static int getId(){
        return id;
    }

    /**
     * Sets the ID of the client.
     * @param newId The new ID
     */
    public static void setId(int newId){
        id = newId;
    }

    /**
     * Response to the END_ROUND command.
     * Resets game status to prepare for a new round.
     */
    public static void endRound(){
        isCurrentPainter = false; // Reset painter role
        Gui.setHeaderLabel("Waiting for new painter...");
        Gui.resetCounter();
    }

    /**
     * Response to the START_ROUND command.
     * Initializes a new round.
     */
    public static void startNewRound(){
        clearPaper();
        Gui.startCountdown(Utility.SystemUtility.ROUND_LENGTH);
    }

    /**
     * Sets the secret word to be painted by the client.
     * @param word The secret word.
     */
    public static void setSecretWord(String word){
        secretWord = word;
    }

    /**
     * Sets the client to be the current painter.
     */
    public static void setCurrentPainter(){
        isCurrentPainter = true;
    }

    /**
     * Check if the client is the current painter.
     * @return True if the client is the current painter; otherwise, false.
     */
    public static boolean isClientCurrentPainter() {return isCurrentPainter;}
}
