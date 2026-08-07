-- CREATE Database userService; -- not needed as this is done in the docker-compose file

CREATE TABLE public.users (
    UserID UUID PRIMARY KEY,
    Gender VARCHAR(50) NOT NULL,
    Bio VARCHAR(500)
);

-- Sample data for insertion
INSERT INTO public.users (UserID, Gender, Bio)
VALUES
(
    'c32f8b42-60e6-4c02-90a7-9143ab37189f',
    'Male',
    'John Doe is a sample user used for testing.'
),
(
    '8ff22c7d-18c7-4ef0-a0ac-988ecb2ac7f5',
    'Female',
    'Jane Doe is a sample user used for testing.'
);a