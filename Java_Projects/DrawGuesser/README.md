# DrawServer and DrawClient

**DrawServer** and **DrawClient** are components of a fun and interactive guessing game where friends can connect to the server for a round of guessing what the painter is drawing. One player acts as the painter, and others try to guess the drawing in real-time. The game continues until everyone has had a chance to draw.

![GUI](Java_Projects/DrawGuesser/GUI.png)

## Prerequisites

- Java 19 or higher is required to run the server and client.

## Running the Application

### 1. Start the Server

To start the **DrawServer**, run the following command with no arguments:
java -jar DrawServer.jar

This will launch the server and it will begin listening for incoming connections at ports 5000 and 5001.

### 2. Start the Client

Once the server is running, you can start the **DrawClient** by providing two arguments:

- **Username**: A unique identifier for the client.
- **Address**: The server address (e.g., `localhost` for local testing).

Run the following command:
java -jar DrawClient.jar <username> <address>

Example:
java -jar DrawClient.jar myUsername localhost


### 3. Interact with the Application

Once the client is connected, you can start interacting with the application. Follow the instructions in the client to begin using the features of the system. Once two or more clients are connected, the game will start!

## Troubleshooting

- If the server and client cannot connect, check that the server is running and verify the address you provided. Also ensure that you're forwarding the ports 5000 and 5001 in your router if you're playing over internet.
- If you cannot run the program, ensure you have the correct version of Java installed.
