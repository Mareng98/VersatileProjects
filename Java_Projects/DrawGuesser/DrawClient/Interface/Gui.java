package Interface;

import Network.MessageWriter;
import Utility.SystemUtility;

import javax.swing.*;
import java.awt.*;

/**
 * A GUI for the drawing guessing game that allows for chatting, displaying information, and painting.
 */
public class Gui extends JFrame{
    private static final JTextArea chatTextArea = new JTextArea(); // Area to display chat messages
    private final JTextField chatInputTextField; // Field for typing new chat messages
    private static final CircularTimer cTimer = new CircularTimer(SystemUtility.ROUND_LENGTH); // Countdown timer
    private static JLabel headerLabel; // Displays a label on top of GUI

    /**
     * Initialize the drawing context and chat box
     */
    public Gui(Paper paper) {
        // Set the layout of the frame
        setLayout(new BorderLayout());

        // Create header panel at the top
        JPanel header = new JPanel();
        headerLabel = new JLabel("Waiting for players...");
        headerLabel.setForeground(Color.WHITE);
        headerLabel.setHorizontalAlignment(SwingConstants.CENTER);
        header.setBackground(new Color(86, 50, 168));
        header.add(headerLabel);
        add(header, BorderLayout.NORTH);

        // Create a panel for the chat area and input box
        JPanel chatPanel = new JPanel(new BorderLayout());
        chatTextArea.setEditable(false); // Stop user from editing chat area
        chatTextArea.setBackground(new Color(248, 248, 255));
        // Have text wrap if it's too long
        chatTextArea.setLineWrap(true);
        chatTextArea.setWrapStyleWord(true);
        // Make it scrollable
        JScrollPane chatScrollPane = new JScrollPane(chatTextArea);
        chatScrollPane.setPreferredSize(new Dimension(250, 780));
        // Add some padding
        chatTextArea.setBorder(BorderFactory.createEmptyBorder(4, 4, 4, 4));
        chatPanel.setBorder(BorderFactory.createEmptyBorder(2, 2, 2, 2));
        chatPanel.add(chatScrollPane, BorderLayout.CENTER); // Add chat area to the center
        // Add the input box at the bottom of the chat panel
        chatInputTextField = new JTextField();
        chatPanel.add(chatInputTextField, BorderLayout.SOUTH); // Add input field to the bottom
        // Add the chat panel to the left
        add(chatPanel, BorderLayout.WEST);

        // Create panel for the drawing canvas
        JPanel drawPanel = new JPanel(new BorderLayout());
        paper.setPreferredSize(new Dimension(650, 780));
        drawPanel.add(paper, BorderLayout.CENTER); // Add the drawing canvas to the right of chat panel

        // Add circular timer for counting down
        cTimer.setPreferredSize(new Dimension(60,60));
        drawPanel.add(cTimer, BorderLayout.SOUTH);
        add(drawPanel,BorderLayout.CENTER);

        // Set frame properties
        setDefaultCloseOperation(EXIT_ON_CLOSE);
        setSize(900, 800);
        setVisible(true);

        // Sends message when pressing enter
        chatInputTextField.addActionListener(e -> sendChatMessage());

        // Add some instructions on how to use the chat to the chat window
        addChatMessage("INSTRUCTIONS: Type in the message box at the bottom left of the window," +
                " press enter to send a message. Add '/g ' in front of your message to make it count " +
                "as a guess at the secret word!");
    }

    /**
     * Sets the header label on the top of the GUI
     * @param text The text to set it to.
     */
    public static void setHeaderLabel(String text){
        headerLabel.setText(text);
    }

    /**
     * Adds a chat message from the input field to the MessageWriter's list of messages to send.
     */
    private void sendChatMessage() {
        String message = chatInputTextField.getText();
        if (message != null && !message.isEmpty()) {
            MessageWriter.addTcpMessage(message); // Add message
            chatInputTextField.setText("");  // Clear input field
        }
    }

    /**
     * Adds a received chat message to the chat area
     * @param message the message to display
     */
    public static void addChatMessage(String message) {
        // Show message with some padding
        chatTextArea.append("\n" + message + "\n");
    }

    /**
     * Sets the timer to the specified amount of time and starts counting down.
     * @param numberOfSeconds the start time in seconds.
     */
    public static void startCountdown(int numberOfSeconds){
        cTimer.startCountdown(numberOfSeconds);
    }

    /**
     * Sets the counter to 0 and stops countdown.
     */
    public static void resetCounter(){
        cTimer.resetCounter();
    }
}
