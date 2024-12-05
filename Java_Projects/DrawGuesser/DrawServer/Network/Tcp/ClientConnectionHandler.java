package Network.Tcp;

import Server.ClientManager;
import Utility.SystemUtility.*;
import Server.Client;
import Server.DrawServer;

import java.io.*;
import java.net.InetAddress;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.List;

/**
 * Handles individual client communication including accepting them through an initial handshake.
 * If they're accepted, the handler continues to process their messages; otherwise, the thread is killed.
 */
public class ClientConnectionHandler extends Thread{

    private final Socket clientSocket;

    /**
     * Initializes the ClientConnectionHandler.
     @param clientSocket The socket to be used for communicating with the client.
     */
    public ClientConnectionHandler(Socket clientSocket){
        this.clientSocket = clientSocket;
    }

    /**
     * Creates a client with username, udp port number, ID, and handles incoming messages from client.
     * The thread that handles outgoing messages to clients is also started here.
     */
    @Override
    public void run() {
        Client client = null;
        ClientMessageSender messageSender = null;

        // Try to create reader and writer
        try (BufferedReader in = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
             PrintWriter out = new PrintWriter(new OutputStreamWriter(clientSocket.getOutputStream(), StandardCharsets.ISO_8859_1), true)) {
            // Get username and UDP port arguments from client
            String response = in.readLine();
            Command command = Command.fromString(response);
            // Validate client arguments
            if(command != null && command.commandType() == CommandType.CLIENT_ARGS){
                client = tryAcceptClient(command.data(), clientSocket.getInetAddress());
            }
            // Verify that the client was accepted
            if(client == null){
                throw new IllegalArgumentException("The client did not provide valid arguments.");
            }
            // Start a thread that sends the messages in the client's tcp message buffer
            messageSender = new ClientMessageSender(out, client);
            messageSender.start();
            // Send the client their ID
            client.addTcpSystemMessage(CommandType.ID, List.of(String.valueOf(client.getId())));
            // Add client to the server's list of clients
            DrawServer.addClient(client);
            // Announce and Log that the client has connected
            String username = client.getUsername();
            ClientManager.broadcastMessage(username + " Connected!");
            System.out.println("Client Connected!\tUsername: " + username +
                    "\tID: " + client.getId() +
                    "\tUDP-port: " + client.getUDP_PORT());

            // Handle incoming client messages while alive, or until the connection closes
            String message;
            while ((message = in.readLine()) != null) {
                // Log all messages sent across the server
                System.out.println("Received message from " + username + "/" + client.getId() + ": " + message);
                // Handle the messages
                handleIncomingClientMessage(message, client);
            }

        } catch (IOException e) {
            System.out.println("Error handling client: " + e);
        } catch (IllegalArgumentException e){
            System.out.println("Client rejected: " + e.getMessage());
        }finally {
            if(client != null){
                // Remove client from clients list and log disconnection if they were created
                DrawServer.removeClient(client);
                System.out.println("Client disconnected: " + client.getUsername());
                ClientManager.broadcastMessage("Client Disconnected: " + client.getUsername());
            }

            try {
                // Kill the client's message sender first
                if(messageSender != null){
                    messageSender.kill();
                    messageSender.join();
                }
                // Then try to close the socket
                if(clientSocket != null){
                    clientSocket.close();
                }
            } catch (IOException | InterruptedException e) {
                System.out.println("Error closing client socket: " + e.getMessage());
            }
        }
    }

    /**
     * Creates a new client if the username and udpPort arguments are valid.
     * @param clientArgs The arguments to use following the structure: {"username","udpPort"}
     * @param clientAddress The IP address of the client (used for logging)
     * @return A client if the validation was successful; otherwise, null.
     */
    private static Client tryAcceptClient(List<String> clientArgs, InetAddress clientAddress){
        boolean usernameOk = false;
        boolean udpPortOk = false;
        int udpPort = 0;
        String username = null;

        // Ensure that there's exactly two arguments
        if(clientArgs.size() == 2){
            username = clientArgs.get(0);
            // Verify valid username
            if(username != null && !username.isEmpty()){
                usernameOk = true;
            }
            // Verify valid udpPort
            try{
                udpPort = Integer.parseInt(clientArgs.get(1));
                if(udpPort >= 0){
                    udpPortOk = true;
                }
            }catch (NumberFormatException e){
                // Ignore
            }
        }

        // Verify that username and udpPort are valid
        if(usernameOk && udpPortOk){
            // Create new client
            return new Client(username, TcpServerController.getNextId(), udpPort, clientAddress);
        } else if (!usernameOk) {
            // Log that username is invalid
            System.out.println("Username invalid for client: " + clientAddress);
        }else if(!udpPortOk){
            // Log that UDP port is invalid
            System.out.println("UDP port invalid for client: " + clientAddress);
        }
        return null;
    }


    /**
     * Interprets a client message and performs some action based on the contents.
     * This method handles client guesses, regular messages, and system messages.
     * @param message The message to be interpreted
     * @param client The client that sent the message
     */
    private static void handleIncomingClientMessage(String message, Client client) {
        // Handle guesses
        if (message.startsWith("/g ")) {
            // Ensure that the client is not the painter
            if(!DrawServer.isCurrentPainter(client.getId())){
                message = message.substring(3);
                //broadcast the guess
                ClientManager.broadcastMessage("(" + client.getPoints() + ") " + client.getUsername() + " - Guessed: " + message);
                // Check if the guess is correct
                if (DrawServer.VerifyGuess(message,client)) {
                    // The guess was correct, Broadcast victory message
                    ClientManager.broadcastMessage("(" + client.getPoints() + ") " + client.getUsername() + " Guessed The Correct Word!!!");
                }
            }else{
                // Inform the client that guessing as the painter is not allowed
                client.addTcpMessage("You can't guess when you're the painter! (Only you can see this)");
            }
        }
        // Handle regular messages
        else if (message.startsWith("/m ")) {
            // Broadcast message to all clients
            message = message.substring(3);
            ClientManager.broadcastMessage("(" + client.getPoints() + ") " + client.getUsername() + " - " + message);
        }
        // Handle system messages
        else if (message.startsWith("/s ")){
            message = message.substring(3);
            if(message.startsWith(CommandType.NEXT_PAINTER_ACK.toString())) {
                // The client accepted to be the next painter, add it to the clients ACK response buffer
                client.addAckResponse(message);
            }
        }
    }
}