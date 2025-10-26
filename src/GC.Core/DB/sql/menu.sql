CREATE TABLE IF NOT EXISTS Menus (
     id INTEGER PRIMARY KEY AUTOINCREMENT,
     hash string NOT NULL,
     type INTEGER NOT NULL,
     title string NOT NULL,
     date DATETIME NOT NULL,
     allergens VARCHAR,
     price REAL NOT NULL
);