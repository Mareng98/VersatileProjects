package Server;

import Utility.SystemUtility.*;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Manages the list of clients and the addition of messages to their outgoing message buffer.
 */
public class ClientManager {
    private static final List<Client> CLIENTS = Collections.synchronizedList(new ArrayList<>()); // List of clients
    private static int currentPainterIndex = -1; // The index of the current painter

    /**
     * Get the number of clients in the list.
     * @return Number of clients
     */
    public static int numOfClients(){
        return CLIENTS.size();
    }

    /**
     * Remove a client from the list.
     * @param client the client to be removed
     */
    protected static void removeClient(Client client) {
        CLIENTS.remove(client);
    }

    /**
     * Add a client to the list.
     * @param client the client to be added
     */
    protected static void addClient(Client client) {
        CLIENTS.add(client);
    }

    /**
     * Get all clients, excluding the client with the matching id.
     * @param id The id used for excluding clients.
     * @return A list of all clients, excluding the clients with the matching id.
     */
    public static List<Client> getClientsExcluding(int id) {
        List<Client> otherClients = new ArrayList<>(); // Create new list
        // Add all clients that don't have a matching id
        for (Client client : CLIENTS) {
            if (client.getId() != id) {
                otherClients.add(client);
            }
        }
        return otherClients;
    }

    /**
     * If there are 2 or more players, currentPainterIndex will point to the position of the next painter
     * in the CLIENTS list; otherwise, currentPainterIndex will be invalid (-1).
     */
    private static void rotatePainterIndex() {
        if (currentPainterIndex < 0 && CLIENTS.size() >= 2) {
            // Initialize currentPainterIndex
            currentPainterIndex = 0;
        } else if (CLIENTS.size() >= 2) {
            // Increment currentPainterIndex with wrap
            currentPainterIndex = (currentPainterIndex + 1) % CLIENTS.size();
        } else {
            // There's not enough clients to start a new game
            currentPainterIndex = -1;
        }
    }

    /**
     * Try to select a client to be the next painter.
     * This will find a client and ask their handler if they're ready to be the next painter,
     * if they do not respond within a certain time-limit, or there are less than 2 clients connected,
     * the operation will fail.
     *
     * @return The next painter if successful; otherwise, null.
     */
    protected static Client tryGetNextPainter() {
        // try to select a new painter
        rotatePainterIndex();
        // Ensure that the index is valid
        if (currentPainterIndex >= 0) {
            // New potential painter was found, try to have them accept the role
            try {
                // Attempt to get the client (they might have disconnected)
                Client nextPainter = CLIENTS.get(currentPainterIndex);
                // Send a next painter request to client
                nextPainter.addTcpSystemMessage(CommandType.NEXT_PAINTER, null);
                // Wait for an ACK response from client for 5 seconds
                if (nextPainter.findAckResponse(CommandType.NEXT_PAINTER_ACK.toString())) {
                    // The client accepted the painter role, return the next painter
                    return nextPainter;
                }
            } catch (IndexOutOfBoundsException e) {
                System.out.println("The client chosen to be the nextPainter disconnected.");
            } catch (InterruptedException e) {
                System.out.println("Main-thread was interrupted when searching for nextPainter ACK: " + e.getMessage());
            }
        }
        // No one could be selected as the next painter
        return null;
    }

    /**
     * Add a system message to all clients' outgoing tcp message buffer without any data.
     * @param commandType The type of command to be sent.
     */
    public synchronized static void broadcastSystemMessage(CommandType commandType) {
        // Create new system command without data
        Command command = new Command(commandType,null);
        for (Client client : CLIENTS) {
            // Add system message to client's buffer
            client.addTcpSystemMessage(command.toString());
        }
    }

    /**
     * Add a system message to all clients' outgoing tcp message buffer with additional data.
     * @param commandType The type of command to be sent.
     * @param data The data to be sent.
     */
    public synchronized static void broadcastSystemMessage(CommandType commandType, String data) {
        // Create new system command without data
        Command command = new Command(commandType,List.of(data));
        for (Client client : CLIENTS) {
            // Add system message to client's buffer
            client.addTcpSystemMessage(command.toString());
        }
    }

    /**
     * Add a regular message to all clients' outgoing tcp message buffer.
     * @param message the message to be sent.
     */
    public synchronized static void broadcastMessage(String message) {
        for (Client client : CLIENTS) {
            // Add message to client's buffer
            client.addTcpMessage(message);
        }
    }
}
