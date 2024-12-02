CREATE TABLE Books (
    book_id INTEGER PRIMARY KEY,
    title TEXT NOT NULL,
    author_id INTEGER NOT NULL,
    category_id INTEGER NOT NULL,
    isbn TEXT NOT NULL UNIQUE,
    publication_year INTEGER NOT NULL,
    copies_available INTEGER NOT NULL,
    FOREIGN KEY (author_id) REFERENCES Authors (author_id),
    FOREIGN KEY (category_id) REFERENCES Categories (category_id)
);

CREATE TABLE Authors (
    author_id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    bio TEXT NOT NULL
);

CREATE TABLE Members (
    member_id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    phone TEXT NOT NULL,
    membership_date DATE NOT NULL
);

CREATE TABLE Loans (
    loan_id INTEGER PRIMARY KEY,
    book_id INTEGER NOT NULL,
    member_id INTEGER NOT NULL,
    loan_date DATE NOT NULL,
    due_date DATE NOT NULL,
    return_date DATE,
    location_id TEXT NOT NULL,
    FOREIGN KEY (book_id) REFERENCES Books (book_id),
    FOREIGN KEY (member_id) REFERENCES Members (member_id),
    FOREIGN KEY (location_id) REFERENCES Location (location_id)
);

CREATE TABLE Categories (
    category_id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    description TEXT NOT NULL
);

CREATE TABLE Location (
    floor INTEGER NOT NULL,
    shelf INTEGER NOT NULL,
    category_id INTEGER NOT NULL,
    book_id INTEGER NOT NULL,
    dewey_Num TEXT NOT NULL,         -- Match NOT NULL constraint in Dewey_Decimal
    location_id TEXT PRIMARY KEY,
    FOREIGN KEY (category_id) REFERENCES Categories (category_id),
    FOREIGN KEY (book_id) REFERENCES Books (book_id),
    FOREIGN KEY (book_id, dewey_Num) REFERENCES Dewey_Decimal (book_id, dewey_num)
);

CREATE TABLE Dewey_Decimal (
    book_id INTEGER NOT NULL,       -- Each book has one unique Dewey classification
    dewey_num TEXT NOT NULL,
    PRIMARY KEY (book_id, dewey_num), -- Composite primary key for unique pairs
    FOREIGN KEY (book_id) REFERENCES Books (book_id)
);

CREATE TABLE Cart (
    cart_id INTEGER PRIMARY KEY,       
    member_id INTEGER NOT NULL,        
    book_id INTEGER NOT NULL,          
    quantity INTEGER NOT NULL DEFAULT 1, 
    added_date DATE NOT NULL DEFAULT (DATE('now')),
    FOREIGN KEY (member_id) REFERENCES Members (member_id),
    FOREIGN KEY (book_id) REFERENCES Books (book_id)
);
