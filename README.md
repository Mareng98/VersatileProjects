# VersatileProjects
This repo showcases some smaller programs I've made.

## Java Projects

### 1. [DrawGuesser : Distributed Drawing Game](Java_Projects/DrawGuesser)

**Description:**
DrawGuesser is a multiplayer drawing game where players connect to a central server to guess what another player is drawing. One player acts as the artist, while the other players try to guess the drawing in real-time. The game is fun and interactive, encouraging creativity and quick thinking among players.

**Features:**
- **Multiplayer support:** Multiple clients can connect to the server to play together.
- **Real-time interaction:** Players can guess what the artist is drawing as soon as it's happening, and chat with each other in the chat window.
- **Server-client architecture:** DrawServer listens for incoming client connections, while DrawClient allows players to interact with the game.
- **Game progression:** Each player gets a turn to be the artist.
- **Multithreading:** The system utilizes multithreading for smooth multiplayer functionality.

---
## AI Projects

### 2. [CNN Tree Species Classification](AI_Projects/tree_identification)

**Description:**
In this analysis, three techniques are used to improve a baseline CNN architecture with the task to classify twelve unique species of trees from a dataset containing textures of tree bark. The techniques used were standardizing the image data, adding batch normalization, and using early stopping. These techniques were proven to be effective for reducing overfitting, decreasing training time,
increasing training stability, and increasing evaluation accuracy through an ablation study. The final model utilizing all of the techniques reached a maximum accuracy of 77.2%, which was an improvement of 12.67% over the baseline model. When extended to classify in distribution and out of distribution samples with the use of K-Means clustering on the features from the output layer, the new model achieved 95.3% accuracy.

**Features:**
- **CNN architecture with PyTorch:** Five different models were tested and compared.
- **Statistical analysis:** Analysis of the result and how to improve the baseline model, including an ablation study.

---

## C# Projects

### 3. [WPF Grocery List Manager](C%23_Projects/GroceryListManager_Project)

**Description:**
The WPF Grocery List Manager is a C# program built using Windows Presentation Foundation (WPF). It provides a user-friendly interface for managing your grocery lists. Users can add, edit, and delete items, making it easy to keep track of what you need to buy during your next shopping trip.

**Features:**
- WPF-based GUI.
- Add, edit, and delete grocery items.
- Save and load grocery lists and grocery types to pick from with ease.

---

### 4. [Windows Forms ToDo TaskManager](C%23_Projects/ToDo_TaskManager)

**Description:**
ToDo TaskManager is a C# program developed using Windows Forms. It serves as a task management application, allowing users to create, organize, and mark tasks as completed. The straightforward interface makes it convenient to stay organized and prioritize your to-do list.

**Features:**
- Windows Forms-based GUI.
- Create, edit, and mark tasks as completed.
- Easy-to-use task organization.

## Future Plans

I plan to feature many more projects in other languages in the future, expanding the variety of programs showcased in this repository.
