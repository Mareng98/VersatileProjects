package Network;

import java.io.BufferedReader;
import java.io.IOException;
import java.util.List;

import Client.DrawClient;
import Utility.SystemUtility.*;
import Interface.Gui;

/**
 * Constantly listens for new messages arriving from the server and handles them.
 */
public class MessageReader extends Thread {
    private final BufferedReader in;
    private volatile boolean alive = true;

    /**
     * Kill this thread.
     */
    public void kill(){
        alive = false;
    }

    /**
     * Initialize the message reader.
     * @param in The buffered reader to read from.
     */
    public MessageReader(BufferedReader in) {
        this.in = in;
    }

    /**
     * Constantly handle new received messages.
     */
    @Override
    public void run() {
        String message;
        try {
            while (alive) {
                // Read messages from the server
                message = in.readLine();
                if(message == null){
                    System.out.println("The server disconnected, exiting program!");
                    DrawClient.setServerConnectionClosed();
                    return;
                }else{
                    // Handle the received message
                    handleMessage(message);
                }
            }
        } catch (IOException e) {
            // Something unexpected went wrong
            System.out.println(e.getMessage());
            DrawClient.setServerConnectionClosed();
        }
    }

    /**
     * Handle a system or regular message.
     * System messages triggers an action, and regular messages are printed to the GUI.
     * @param message The message to handle.
     */
    private void handleMessage(String message) {
        // Check if the message is a system message
        if (message.startsWith("/s ")) {
            // Remove "/s " prefix from the message
            message = message.substring(3);

            // Try to parse command
            Command command = Command.fromString(message);
            if (command == null) {
                System.out.println("Unknown system command.");
                return;
            }

            // Handles different system messages based on their commandType
            switch (command.commandType()) {
                case NEXT_PAINTER -> { // Server selected us as the next painter
                    DrawClient.setCurrentPainter();
                    // Tell the server we accept the role with an ACK command
                    MessageWriter.addTcpSystemMessage(new Command(CommandType.NEXT_PAINTER_ACK, null).toString());
                }
                case END_ROUND -> { // End current round
                    DrawClient.endRound();
                }
                case INTERRUPT_ROUND -> { // Interrupt current round
                    String interruptMessage = String.join(":", command.data()); // Reconstruct message from data
                    Gui.addChatMessage(interruptMessage);
                }
                case ID -> { // Set ID
                    try {
                        int clientId = Integer.parseInt(command.data().get(0)); // Get ID from command data
                        DrawClient.setId(clientId);
                    } catch (NumberFormatException e) {
                        System.out.println("The received ID was not a valid integer!\n" + e.getMessage());
                        DrawClient.setServerConnectionClosed();
                    }
                }
                case SECRET_WORD -> { // Set secret word
                    DrawClient.setSecretWord(command.data().get(0)); // Set secret word from command data
                }
                case NEW_ROUND -> { // Prepare for new round
                    DrawClient.startNewRound();
                }
                case START_ROUND -> { // Start a new round
                    String painterMessage = command.data().get(0);
                    if (DrawClient.isClientCurrentPainter()) {
                        Gui.setHeaderLabel("The word you have to paint is... " + DrawClient.getSecretWord() + "!");
                    } else {
                        Gui.setHeaderLabel(painterMessage + " is painting!");
                    }
                    Gui.addChatMessage("The New Painter Is " + painterMessage + "!");
                }
                case JOIN_ROUND -> { // Join an ongoing round
                    // Try to parse the command
                    Command joinRoundCommand = Command.fromString(message);
                    if(joinRoundCommand != null){
                        try{
                            // Try to parse the data
                            List<String> data = joinRoundCommand.data();
                            if(data.size() == 2){
                                String username = data.get(0);
                                int remainingRoundTime = Integer.parseInt(data.get(1));
                                // Set header and set timer to the remaining time of the round
                                Gui.setHeaderLabel(username + " is painting!");
                                Gui.startCountdown(remainingRoundTime);
                        }else{
                            System.out.println("Join round message was malformed with size: " + data.size());
                        }
                        }catch (NumberFormatException e){
                            System.out.println("Join round message was malformed: " + e.getMessage());
                        }
                    }
                }
            }
        }
        // Handle regular messages by printing them to GUI
        else if (message.startsWith("/m ")) {
            message = message.substring(3);
            Gui.addChatMessage(message);
        }
    }
}
