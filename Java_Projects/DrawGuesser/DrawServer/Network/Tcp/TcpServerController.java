package Network.Tcp;

import java.io.*;
import java.net.ServerSocket;
import java.net.Socket;

/**
 * Handles all TCP communication between the server and client, and initializes new clients.
 */
public class TcpServerController implements Runnable {
    private static int nextId = 0; // An incrementing number used for client id
    private final int port; // Server's TCP port number
    private boolean alive; // TCP resources will eventually be released if this is set to false
    private ServerSocket tcpServerSocket; // Socket to accept client connections through

    /**
     * Initialize the TCP handler
     * @param port the port number to be used for the server socket
     */
    public TcpServerController(int port) {
        this.port = port;
        alive = true;
    }

    /**
     * Kill the TcpHandler and release all resources.
     */
    public void kill() {
        alive = false;
        try {
            // Close the ServerSocket to interrupt the blocking accept() action and kill thread
            if (tcpServerSocket != null && !tcpServerSocket.isClosed()) {
                tcpServerSocket.close();
            }
        } catch (IOException e) {
            // Ignore
        }
    }

    /**
     * Accepts new clients and creates new threads to handle their communication.
     */
    @Override
    public void run() {
        try (ServerSocket tcpServerSocket = new ServerSocket(port)) {
            // Save variable in case we want to kill the TcpHandler
            this.tcpServerSocket = tcpServerSocket;
            System.out.println("Server is listening for TCP connections on port " + port);
            // Handle new client connections while alive
            while (alive) {
                Socket clientSocket = tcpServerSocket.accept(); // Accept new connection
                System.out.println("Accepted TCP connection from " + clientSocket.getInetAddress());
                // Create a new thread that handles the new client
                ClientConnectionHandler clientConnectionHandler = new ClientConnectionHandler(clientSocket);
                clientConnectionHandler.start();
            }
        } catch (IOException e) {
            System.out.println("ServerSocket closed!");
        }
    }

    /**
     * Increment nextId by 1
     * @return The incremented nextId
     */
    public static synchronized int getNextId() {
        return nextId++;
    }
}
