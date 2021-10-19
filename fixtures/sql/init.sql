-- SCHEMA
CREATE TABLE github_users
(
    id serial PRIMARY KEY,
    created timestamptz DEFAULT now() NOT NULL,
    modified timestamptz DEFAULT now() NOT NULL,
    name text NOT NULL,
    userid text NOT NULL,
    vanity_name text NOT NULL
);

-- SEED
INSERT INTO github_users
    (id, name, userid, vanity_name)
VALUES
    (1, 'Brandon Atkinson', 'atkinsonbg', 'atkinsonbg');


ALTER SEQUENCE github_users_id_seq RESTART WITH 100;