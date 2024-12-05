package Utility;

import java.util.Arrays;
import java.util.List;
import java.util.StringJoiner;

/**
 * Used for bridging the logic between the server and client side code.
 * Specifies the structure of commands and the round length.
 */
public class SystemUtility {

    public static final int ROUND_LENGTH = 120; // The length of a round in seconds

    /**
     * Command types for all the system messages that the system can send between client and server.
     */
    public enum CommandType {
        NEXT_PAINTER, // Sent by server: Request client to accept painter role
        NEXT_PAINTER_ACK, // Sent by client: Accepted the painter role
        ID, // Sent by server: Gives a client an ID
        SECRET_WORD, // Sent by server: Gives a client the secret word
        NEW_ROUND, // Sent by server: Inform client that a new round is about to start
        START_ROUND, // Sent by server: Starts a new round
        END_ROUND, // Sent by server: Ends an active round
        JOIN_ROUND, // Sent by server: Allows client to join an ongoing round
        INTERRUPT_ROUND, // Sent by server: Interrupts a round
        CLIENT_ARGS; // Sent by client: initial handshake
    }

    /**
     * Command that has a type and a list of optional data
     * @param commandType the type of the command.
     * @param data list of optional data.
     */
    public record Command(CommandType commandType, List<String> data) {

        // Formats the command to a string that follow the structure "commandType:data1:data2:etc"
        @Override
        public String toString() {
            StringJoiner joiner = new StringJoiner(":");
            joiner.add(commandType.name()); // Add the command type
            // If there's additional data, add it
            if (data != null && !data.isEmpty()) {
                for (String datum : data) {
                    joiner.add(datum); // Add each data item to the string
                }
            }
            return joiner.toString();
        }

        /**
         * Parse a command string to extract the command type and list of data
         * @param message the string representation of the command
         * @return The command if it exists; otherwise, null.
         */
        public static Command fromString(String message) {
            // Split string into command type and potential data components
            String[] components = message.split(":");
            // Ensure that message isn't empty
            if (components.length > 0) {
                String header = components[0]; // First component is the command type
                // Check if header matches any known commands
                for (CommandType type : CommandType.values()) {
                    if (header.equals(type.name())) {
                        String[] data;
                        if (components.length > 1) {
                            // Extract the data from the remaining part of the string
                            data = Arrays.copyOfRange(components, 1, components.length);
                        } else {
                            // There's no data, avoid null pointer exception by creating an empty arry
                            data = new String[0];
                        }
                        // Return a valid command
                        return new Command(type, List.of(data));
                    }
                }
            }
            // No existing command matches this string format.
            return null;
        }
    }
}
