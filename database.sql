CREATE TABLE "checks" (
	"id"	INTEGER,
	"date"	TEXT NOT NULL,
	"site_id"	INTEGER NOT NULL,
	"position"	INTEGER NOT NULL,
	"middle_position"	INTEGER NOT NULL DEFAULT 0,
	PRIMARY KEY("id" AUTOINCREMENT)
)
 
CREATE TABLE "metrika" (
	"id"	INTEGER,
	"id_domain"	INTEGER NOT NULL,
	"google"	INTEGER DEFAULT 0,
	"yandex"	INTEGER DEFAULT 0,
	"other"	INTEGER DEFAULT 0,
	"date"	TEXT DEFAULT '',
	PRIMARY KEY("id" AUTOINCREMENT)
)

CREATE TABLE "domains" (
	"id"	INTEGER,
	"name"	TEXT NOT NULL,
	"rus_name"	TEXT DEFAULT '',
	"expire_date"	TEXT NOT NULL,
	"ip"	TEXT DEFAULT '',
	"has_site"	TEXT DEFAULT '',
	"visits"	INTEGER DEFAULT 0,
	"sales"	INTEGER DEFAULT 0,
	"comments"	TEXT DEFAULT '',
	"counter_id"	INTEGER DEFAULT 0,
	"show"	INTEGER DEFAULT 1,
	PRIMARY KEY("id" AUTOINCREMENT)
)

CREATE TABLE "sites" (
	"id"	INTEGER,
	"date"	TEXT,
	"page_address"	TEXT NOT NULL,
	"query"	TEXT,
	"position_current"	INTEGER,
	"position_middle_current"	INTEGER DEFAULT 0,
	"position_previous"	INTEGER,
	"position_midlle_previous"	INTEGER DEFAULT 0,
	"url_in_search"	TEXT,
	"comment"	TEXT,
	"status"	TEXT,
	"row_position"	INTEGER DEFAULT 0,
	"domain_id"	INTEGER DEFAULT 0,
	"cpa_id"	INTEGER DEFAULT 0,
	"hosting_id"	INTEGER DEFAULT 0,
	PRIMARY KEY("id" AUTOINCREMENT)
)

CREATE TABLE "hosting_list" (
	"id"	INTEGER,
	"name"	TEXT NOT NULL,
	"ip"	TEXT DEFAULT '',
	PRIMARY KEY("id" AUTOINCREMENT)
)


CREATE TABLE "cpa_list" (
	"id"	INTEGER,
	"name"	TEXT NOT NULL,
	"login" TEXT DEFAULT '',
	"url" TEXT DEFAULT '',
	"script"	TEXT DEFAULT '',
	"description"	TEXT DEFAULT '',
	PRIMARY KEY("id" AUTOINCREMENT)
)