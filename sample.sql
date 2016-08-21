DROP TABLE IF EXISTS sample;
CREATE TABLE sample (
    id integer NOT NULL,
    metadata jsonb
);
ALTER TABLE sample OWNER TO postgres;

ALTER TABLE ONLY sample
    ADD CONSTRAINT sample_pkey PRIMARY KEY (id);

INSERT INTO sample (id, metadata) VALUES (1, '{}'::jsonb);
INSERT INTO sample (id, metadata) VALUES (2, '{"ok": "test"}'::jsonb);
INSERT INTO sample (id, metadata) VALUES (3, '"Failure"'::jsonb);