CREATE TABLE IF NOT EXISTS BillingTransactions (
     id INTEGER PRIMARY KEY AUTOINCREMENT,
     type INTEGER NOT NULL,
     date DATETIME NOT NULL,
     hash string NOT NULL
);

CREATE TABLE IF NOT EXISTS BillingPositions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  name TEXT NOT NULL,
  quantity INTEGER NOT NULL,
  unit_price REAL NOT NULL,
  support REAL NOT NULL,
  transaction_id INTEGER NOT NULL,
  FOREIGN KEY (transaction_id) REFERENCES BillingTransactions(id)
);
