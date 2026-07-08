-- CREATE Database userService; -- not needed as this is done in the docker-compose file

CREATE TABLE public.users (
    userid UUID PRIMARY KEY,
    personname VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL,
    password VARCHAR(50) NOT NULL,
    gender VARCHAR(50) NOT NULL,
    CONSTRAINT email_unique UNIQUE (email)
);


-- Sample data for insertion
INSERT INTO public.users (userid, email, personname, gender, password)
VALUES 
('c32f8b42-60e6-4c02-90a7-9143ab37189f', 'test1@example.com', 'John Doe', 'Male', 'password1'),
('8ff22c7d-18c7-4ef0-a0ac-988ecb2ac7f5', 'test2@example.com', 'Jane Smith', 'Female', 'password2');