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
	"competitor"	TEXT,
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

CREATE TABLE "site_analysis_data" (
	"id"	INTEGER,
	"site_id1"	INTEGER NOT NULL,
	"check_date"	TEXT NOT NULL,
	"page_url"	TEXT,
	"strategy"	TEXT,
	"fetch_time"	TEXT,
	"psi_perf_score"	REAL,
	"psi_seo_score"	REAL,
	"psi_bp_score"	REAL,
	"psi_a11y_score"	REAL,
	"psi_lcp_ms"	REAL,
	"psi_cls"	REAL,
	"psi_inp_ms"	REAL,
	"psi_tbt_ms"	REAL,
	"psi_ttfb_ms"	REAL,
	"psi_fcp_ms"	REAL,
	"psi_si_ms"	REAL,
	"psi_bytes"	REAL,
	"psi_req_cnt"	INTEGER,
	"psi_unused_js_b"	REAL,
	"psi_unused_css_b"	REAL,
	"psi_offscr_img_b"	REAL,
	"psi_modern_img_b"	REAL,
	"psi_opt_img_b"	REAL,
	"word_keyword"	TEXT,
	"word_total_words"	INTEGER,
	"word_total_sentences"	INTEGER,
	"word_total_paragraphs"	INTEGER,
	"word_total_words_in_paragraphs"	INTEGER,
	"word_h1_count"	INTEGER,
	"word_h2_count"	INTEGER,
	"word_h3_count"	INTEGER,
	"word_h4_count"	INTEGER,
	"word_h5_count"	INTEGER,
	"word_total_words_in_headers"	INTEGER,
	"word_total_words_in_title"	INTEGER,
	"word_total_words_in_description"	INTEGER,
	"word_image_count"	INTEGER,
	"word_inner_links"	INTEGER,
	"word_outer_links"	INTEGER,
	"word_total_words_in_links"	INTEGER,
	"word_kw_words_count"	INTEGER,
	"word_kw_words_in_title"	INTEGER,
	"word_kw_words_in_description"	INTEGER,
	"word_kw_words_in_headers"	INTEGER,
	"word_kw_words_in_alt"	INTEGER,
	"word_kw_words_in_text"	INTEGER,
	"word_tokens_ratio"	REAL,
	"word_kincaid_score"	REAL,
	"word_flesch_reading_ease"	REAL,
	"word_gunning_fog"	REAL,
	"word_smog_index"	REAL,
	"word_ari"	REAL,
	"word_main_keyword_density"	REAL,
	"raw_json"	TEXT,
	PRIMARY KEY("id" AUTOINCREMENT)
)
