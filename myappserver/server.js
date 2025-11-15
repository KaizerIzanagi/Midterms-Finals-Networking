const express = require("express");
const app = express();

app.use(express.json());

// Fake database (in-memory)
let players = {};

// -----------------------------
// POST - Register Player
// -----------------------------
app.post("/register", (req, res) => {
    const { PlayerID, PlayerName, Email } = req.body;

    if (!PlayerID || !PlayerName || !Email) {
        return res.status(400).json({ error: "Missing fields!" });
    }

    if (players[PlayerID]) {
        return res.status(400).json({ error: "Player already exists" });
    }

    players[PlayerID] = {
        PlayerID,
        PlayerName,
        Email,
        Level: 1
    };

    res.json({ status: "registered", data: players[PlayerID] });
});

// -----------------------------
// POST - Login Player
// -----------------------------
app.post("/login", (req, res) => {
    const { PlayerID } = req.body;

    if (!players[PlayerID]) {
        return res.status(404).json({ error: "Player not found!" });
    }

    res.json({ status: "logged_in", data: players[PlayerID] });
});

// -----------------------------
// GET - Get Player
// -----------------------------
app.get("/player/:PlayerID", (req, res) => {
    const { PlayerID } = req.params;

    if (!players[PlayerID]) {
        return res.status(404).json({ error: "Player not found!" });
    }

    res.json(players[PlayerID]);
});

// -----------------------------
// PUT - Update Player
// -----------------------------
app.put("/player/:PlayerID", (req, res) => {
    const { PlayerID } = req.params;

    if (!players[PlayerID]) {
        return res.status(404).json({ error: "Player not found!" });
    }

    const { PlayerName, Email, Level } = req.body;

    if (PlayerName !== undefined) players[PlayerID].PlayerName = PlayerName;
    if (Email !== undefined) players[PlayerID].Email = Email;
    if (Level !== undefined) players[PlayerID].Level = Level;

    res.json({ status: "updated", data: players[PlayerID] });
});

// -----------------------------
// DELETE - Delete Player
// -----------------------------
app.delete("/player/:PlayerID", (req, res) => {
    const { PlayerID } = req.params;

    if (!players[PlayerID]) {
        return res.status(404).json({ error: "Player not found" });
    }

    delete players[PlayerID];

    res.json({ status: "deleted", PlayerID });
});

// Start server
const PORT = 3000;
app.listen(PORT, () => {
    console.log("Server running at http://localhost:" + PORT);
});