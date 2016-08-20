DROP TABLE IF EXISTS sample

CREATE TABLE sample (
    id integer NOT NULL,
    metadata jsonb,
);

ALTER TABLE comment OWNER TO postgres;

INSERT INTO sample (id, metadata) VALUES (1, '{}');
INSERT INTO sample (id, metadata) VALUES (1, '{"ok": "test"}');
INSERT INTO sample (id, metadata) VALUES (1, 'Failure');
