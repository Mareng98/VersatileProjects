package Server;

import Utility.SystemUtility.*;

import java.net.InetAddress;
import java.util.*;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;
/**
 * Contains all necessary information about the client and manages the clients ACK responses and message buffer
 * that is used for sending them messages.
 */
public class Client {
    private final Queue<String> ackQueue = new LinkedList<>(); // Queue for ACK responses
    // Thread safe queue for adding outgoing messages to be sent to the client
    private final LinkedBlockingQueue<String> tcpMessagesToSend = new LinkedBlockingQueue<>();
    private final String username; // The username of the client
    private final int id; // The id of the client
    private final int UDP_PORT; // The UDP port of the client
    private final InetAddress address; // The IP address of the client
    private int points; // The game points of the client

    /**
     * Create a client with all necessary information about them
     * @param username username of the client
     * @param id unique id of the client
     * @param UDP_PORT UDP port of the client
     * @param address IP address of the client
     */
    public Client(String username, int id, int UDP_PORT, InetAddress address){
        this.username = username;
        this.id = id;
        this.UDP_PORT = UDP_PORT;
        this.address = address;
    }

    /**
     * Add an incoming ACK response from client
     * @param message the ACK response
     */
    public void addAckResponse(String message){
        ackQueue.add(message);
    }

    /**
     * Attempts to find a specific ACK response by client until it is either found or it times out (5 seconds).
     * (Old unhandled messages could be removed automatically)
     * @param searchString the response to be found.
     * @return true if the response was found; otherwise, false.
     * @throws InterruptedException The thread was unexpectedly interrupted.
     */
    public boolean findAckResponse(String searchString) throws InterruptedException {
        long timeout = 5_000_000_000L; // Timeout 5 seconds in nanoseconds
        long startTime = System.nanoTime(); // Time to compare against
        int pollInterval = 100; // Timeout 100 ms

        // Try to find an ACK response from client for 5 seconds
        while ((System.nanoTime() - startTime) < timeout) {
            // Search through the queue
            for (String response : ackQueue) {
                if (response.equals(searchString)) {
                    // The response has been received, remove it from the queue
                    ackQueue.remove(response);
                    return true;
                }
            }
            // Sleep for a short time before checking again
            Thread.sleep(pollInterval);
        }

        // ACK response wasn't found
        return false;
    }

    /**
     * Add game points to client.
     * @param addition the amount of points to add.
     */
    public void addPoints(int addition){
        points += addition;
    }

    /**
     * Get the unique identifier of the client.
     * @return the ID.
     */
    public int getId(){
        return id;
    }

    /**
     * Get the username of the client.
     * @return the username.
     */
    public String getUsername(){
        return username;
    }

    /**
     * Get the game points of the client.
     * @return the number of points.
     */
    public int getPoints(){
        return points;
    }

    /**
     * Get the UDP port number that the client uses.
     * @return the UDP port number.
     */
    public int getUDP_PORT(){return UDP_PORT;}

    /**
     * Get the IP address of the client.
     * @return the IP address.
     */
    public InetAddress getAddress(){return address;}

    /**
     * Add a regular TCP message to the clients message sending buffer.
     * @param message the message to add.
     */
    public void addTcpMessage(String message) {
        tcpMessagesToSend.add("/m " + message);
    }

    /**
     * Add a System TCP message to the clients message sending buffer.
     * @param message the message to add.
     */
    public void addTcpSystemMessage(String message) {
        tcpMessagesToSend.add("/s " + message);
    }

    /**
     * Add a system message to the clients outgoing tcp message buffer.
     * @param commandType the type of the system message.
     * @param optionalData optional data.
     */
    public void addTcpSystemMessage(CommandType commandType, List<String> optionalData) {
        Command command = new Command(commandType,optionalData); // Create command
        tcpMessagesToSend.add("/s " + command); // Add system message as a string
    }

    /**
     * Take a message to be sent from the TCP message buffer, waits for an available message for 1 second.
     * @return The message to be sent if a message was found; otherwise, null.
     */
    public String pollTcpMessageToSend() {
        try {
            return tcpMessagesToSend.poll(1000,TimeUnit.SECONDS);  // Waits for a message for 1 second
        } catch (InterruptedException e) {
            return null;
        }
    }

}
