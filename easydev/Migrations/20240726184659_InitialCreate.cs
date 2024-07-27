using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace easydev.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.EnsureSchema(
                name: "realtime");

            migrationBuilder.EnsureSchema(
                name: "graphql");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:DB", "Mysql,SQL Server,PostgreSQL")
                .Annotation("Npgsql:Enum:HTTP METHOD", "GET,POST,DELETE,PUT,PATCH")
                .Annotation("Npgsql:Enum:auth.aal_level", "aal1,aal2,aal3")
                .Annotation("Npgsql:Enum:auth.code_challenge_method", "s256,plain")
                .Annotation("Npgsql:Enum:auth.factor_status", "unverified,verified")
                .Annotation("Npgsql:Enum:auth.factor_type", "totp,webauthn")
                .Annotation("Npgsql:Enum:auth.one_time_token_type", "confirmation_token,reauthentication_token,recovery_token,email_change_token_new,email_change_token_current,phone_change_token")
                .Annotation("Npgsql:Enum:pgsodium.key_status", "default,valid,invalid,expired")
                .Annotation("Npgsql:Enum:pgsodium.key_type", "aead-ietf,aead-det,hmacsha512,hmacsha256,auth,shorthash,generichash,kdf,secretbox,secretstream,stream_xchacha20")
                .Annotation("Npgsql:Enum:realtime.action", "INSERT,UPDATE,DELETE,TRUNCATE,ERROR")
                .Annotation("Npgsql:Enum:realtime.equality_op", "eq,neq,lt,lte,gt,gte,in")
                .Annotation("Npgsql:PostgresExtension:extensions.pg_stat_statements", ",,")
                .Annotation("Npgsql:PostgresExtension:extensions.pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:extensions.pgjwt", ",,")
                .Annotation("Npgsql:PostgresExtension:extensions.uuid-ossp", ",,")
                .Annotation("Npgsql:PostgresExtension:graphql.pg_graphql", ",,")
                .Annotation("Npgsql:PostgresExtension:pgsodium.pgsodium", ",,")
                .Annotation("Npgsql:PostgresExtension:vault.supabase_vault", ",,");

            migrationBuilder.CreateSequence<int>(
                name: "seq_schema_version",
                schema: "graphql",
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_log_entries_pkey", x => x.id);
                },
                comment: "Auth: Audit trail for user actions.");

            migrationBuilder.CreateTable(
                name: "buckets",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner = table.Column<Guid>(type: "uuid", nullable: true, comment: "Field is deprecated, use owner_id instead"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: true, defaultValue: false),
                    avif_autodetection = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    file_size_limit = table.Column<long>(type: "bigint", nullable: true),
                    allowed_mime_types = table.Column<List<string>>(type: "text[]", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("buckets_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Database",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DBEngine = table.Column<string>(type: "text", nullable: true),
                    host = table.Column<string>(type: "character varying", nullable: true),
                    password = table.Column<string>(type: "character varying", nullable: true),
                    user = table.Column<string>(type: "character varying", nullable: true),
                    database = table.Column<string>(type: "character varying", nullable: true),
                    port = table.Column<string>(type: "character varying", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Database_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flow_state",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    auth_code = table.Column<string>(type: "text", nullable: false),
                    code_challenge = table.Column<string>(type: "text", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    provider_access_token = table.Column<string>(type: "text", nullable: true),
                    provider_refresh_token = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    authentication_method = table.Column<string>(type: "text", nullable: false),
                    auth_code_issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("flow_state_pkey", x => x.id);
                },
                comment: "stores metadata for pkce logins");

            migrationBuilder.CreateTable(
                name: "instances",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_base_config = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("instances_pkey", x => x.id);
                },
                comment: "Auth: Manages users across multiple sites.");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    idUser = table.Column<long>(type: "bigint", nullable: true),
                    idProject = table.Column<long>(type: "bigint", nullable: true),
                    query = table.Column<string>(type: "text", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requestDuration = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "realtime",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic = table.Column<string>(type: "text", nullable: false),
                    extension = table.Column<string>(type: "text", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp(0) without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp(0) without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("messages_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "migrations",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("migrations_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                schema: "auth",
                columns: table => new
                {
                    version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("schema_migrations_pkey", x => x.version);
                },
                comment: "Auth: Manages updates to the auth system.");

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                schema: "realtime",
                columns: table => new
                {
                    version = table.Column<long>(type: "bigint", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp(0) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("schema_migrations_pkey", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "sso_providers",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<string>(type: "text", nullable: true, comment: "Auth: Uniquely identifies a SSO provider according to a user-chosen resource ID (case insensitive), useful in infrastructure as code."),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sso_providers_pkey", x => x.id);
                },
                comment: "Auth: Manages SSO identity provider information; see saml_providers for SAML.");

            migrationBuilder.CreateTable(
                name: "subscription",
                schema: "realtime",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claims = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc'::text, now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    name = table.Column<string>(type: "character varying", nullable: false),
                    mail = table.Column<string>(type: "character varying", nullable: false),
                    password = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aud = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    encrypted_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmation_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    confirmation_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    recovery_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recovery_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_change_token_new = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_change = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_change_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    raw_app_meta_data = table.Column<string>(type: "jsonb", nullable: true),
                    raw_user_meta_data = table.Column<string>(type: "jsonb", nullable: true),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true, defaultValueSql: "NULL::character varying"),
                    phone_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone_change = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::character varying"),
                    phone_change_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    phone_change_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, computedColumnSql: "LEAST(email_confirmed_at, phone_confirmed_at)", stored: true),
                    email_change_token_current = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    email_change_confirm_status = table.Column<short>(type: "smallint", nullable: true, defaultValue: (short)0),
                    banned_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reauthentication_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    reauthentication_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_sso_user = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Auth: Set this column to true when the account comes from SSO. These accounts can have duplicate emails."),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_anonymous = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                },
                comment: "Auth: Stores user login data within a secure schema.");

            migrationBuilder.CreateTable(
                name: "objects",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bucket_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    owner = table.Column<Guid>(type: "uuid", nullable: true, comment: "Field is deprecated, use owner_id instead"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    path_tokens = table.Column<List<string>>(type: "text[]", nullable: true, computedColumnSql: "string_to_array(name, '/'::text)", stored: true),
                    version = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("objects_pkey", x => x.id);
                    table.ForeignKey(
                        name: "objects_bucketId_fkey",
                        column: x => x.bucket_id,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "s3_multipart_uploads",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    in_progress_size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    upload_signature = table.Column<string>(type: "text", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false, collation: "C"),
                    version = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("s3_multipart_uploads_pkey", x => x.id);
                    table.ForeignKey(
                        name: "s3_multipart_uploads_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "saml_providers",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: false),
                    metadata_xml = table.Column<string>(type: "text", nullable: false),
                    metadata_url = table.Column<string>(type: "text", nullable: true),
                    attribute_mapping = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name_id_format = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("saml_providers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "saml_providers_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Manages SAML Identity Provider connections.");

            migrationBuilder.CreateTable(
                name: "saml_relay_states",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<string>(type: "text", nullable: false),
                    for_email = table.Column<string>(type: "text", nullable: true),
                    redirect_to = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    flow_state_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("saml_relay_states_pkey", x => x.id);
                    table.ForeignKey(
                        name: "saml_relay_states_flow_state_id_fkey",
                        column: x => x.flow_state_id,
                        principalSchema: "auth",
                        principalTable: "flow_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "saml_relay_states_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Contains SAML Relay State information for each Service Provider initiated login.");

            migrationBuilder.CreateTable(
                name: "sso_domains",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sso_domains_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sso_domains_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Manages SSO email address domain mapping to an SSO Identity Provider.");

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    title = table.Column<string>(type: "character varying", nullable: true),
                    description = table.Column<string>(type: "character varying", nullable: true),
                    key = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    idUser = table.Column<long>(type: "bigint", nullable: true),
                    IDDatabase = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("project_pkey", x => x.id);
                    table.ForeignKey(
                        name: "project_IDDatabase_fkey",
                        column: x => x.IDDatabase,
                        principalTable: "Database",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "project_idUser_fkey",
                        column: x => x.idUser,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "identities",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_data = table.Column<string>(type: "jsonb", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower((identity_data ->> 'email'::text))", stored: true, comment: "Auth: Email is a generated column that references the optional email property in the identity_data")
                },
                constraints: table =>
                {
                    table.PrimaryKey("identities_pkey", x => x.id);
                    table.ForeignKey(
                        name: "identities_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Stores identities associated to a user.");

            migrationBuilder.CreateTable(
                name: "mfa_factors",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    secret = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mfa_factors_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mfa_factors_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores metadata about factors");

            migrationBuilder.CreateTable(
                name: "one_time_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    relates_to = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("one_time_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "one_time_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    factor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    not_after = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Auth: Not after is a nullable column that contains a timestamp after which the session should be regarded as expired."),
                    refreshed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    tag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sessions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sessions_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Stores session data associated to a user.");

            migrationBuilder.CreateTable(
                name: "s3_multipart_uploads_parts",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    upload_id = table.Column<string>(type: "text", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    part_number = table.Column<int>(type: "integer", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false, collation: "C"),
                    etag = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("s3_multipart_uploads_parts_pkey", x => x.id);
                    table.ForeignKey(
                        name: "s3_multipart_uploads_parts_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "s3_multipart_uploads_parts_upload_id_fkey",
                        column: x => x.upload_id,
                        principalSchema: "storage",
                        principalTable: "s3_multipart_uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endpoint",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    httpMethod = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "character varying", nullable: true),
                    query = table.Column<string>(type: "character varying", nullable: true),
                    idProject = table.Column<long>(type: "bigint", nullable: true),
                    @params = table.Column<string>(name: "params", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Endpoint_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Endpoint_idProject_fkey",
                        column: x => x.idProject,
                        principalTable: "project",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mfa_challenges",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mfa_challenges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mfa_challenges_auth_factor_id_fkey",
                        column: x => x.factor_id,
                        principalSchema: "auth",
                        principalTable: "mfa_factors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores metadata about challenge requests made");

            migrationBuilder.CreateTable(
                name: "mfa_amr_claims",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    authentication_method = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("amr_id_pk", x => x.id);
                    table.ForeignKey(
                        name: "mfa_amr_claims_session_id_fkey",
                        column: x => x.session_id,
                        principalSchema: "auth",
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores authenticator method reference claims for multi factor authentication");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    revoked = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    parent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "refresh_tokens_session_id_fkey",
                        column: x => x.session_id,
                        principalSchema: "auth",
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Store of tokens used to refresh JWT tokens once they expire.");

            migrationBuilder.CreateIndex(
                name: "audit_logs_instance_id_idx",
                schema: "auth",
                table: "audit_log_entries",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "bname",
                schema: "storage",
                table: "buckets",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Endpoint_idProject",
                table: "Endpoint",
                column: "idProject");

            migrationBuilder.CreateIndex(
                name: "flow_state_created_at_idx",
                schema: "auth",
                table: "flow_state",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_auth_code",
                schema: "auth",
                table: "flow_state",
                column: "auth_code");

            migrationBuilder.CreateIndex(
                name: "idx_user_id_auth_method",
                schema: "auth",
                table: "flow_state",
                columns: new[] { "user_id", "authentication_method" });

            migrationBuilder.CreateIndex(
                name: "identities_email_idx",
                schema: "auth",
                table: "identities",
                column: "email")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "identities_provider_id_provider_unique",
                schema: "auth",
                table: "identities",
                columns: new[] { "provider_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "identities_user_id_idx",
                schema: "auth",
                table: "identities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "messages_topic_index",
                schema: "realtime",
                table: "messages",
                column: "topic");

            migrationBuilder.CreateIndex(
                name: "mfa_amr_claims_session_id_authentication_method_pkey",
                schema: "auth",
                table: "mfa_amr_claims",
                columns: new[] { "session_id", "authentication_method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mfa_challenges_factor_id",
                schema: "auth",
                table: "mfa_challenges",
                column: "factor_id");

            migrationBuilder.CreateIndex(
                name: "mfa_challenge_created_at_idx",
                schema: "auth",
                table: "mfa_challenges",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "factor_id_created_at_idx",
                schema: "auth",
                table: "mfa_factors",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "mfa_factors_user_friendly_name_unique",
                schema: "auth",
                table: "mfa_factors",
                columns: new[] { "friendly_name", "user_id" },
                unique: true,
                filter: "(TRIM(BOTH FROM friendly_name) <> ''::text)");

            migrationBuilder.CreateIndex(
                name: "mfa_factors_user_id_idx",
                schema: "auth",
                table: "mfa_factors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "migrations_name_key",
                schema: "storage",
                table: "migrations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "bucketid_objname",
                schema: "storage",
                table: "objects",
                columns: new[] { "bucket_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_objects_bucket_id_name",
                schema: "storage",
                table: "objects",
                columns: new[] { "bucket_id", "name" })
                .Annotation("Relational:Collation", new[] { null, "C" });

            migrationBuilder.CreateIndex(
                name: "name_prefix_search",
                schema: "storage",
                table: "objects",
                column: "name")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_one_time_tokens_user_id",
                schema: "auth",
                table: "one_time_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "one_time_tokens_relates_to_hash_idx",
                schema: "auth",
                table: "one_time_tokens",
                column: "relates_to")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "one_time_tokens_token_hash_hash_idx",
                schema: "auth",
                table: "one_time_tokens",
                column: "token_hash")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_project_IDDatabase",
                table: "project",
                column: "IDDatabase");

            migrationBuilder.CreateIndex(
                name: "IX_project_idUser",
                table: "project",
                column: "idUser");

            migrationBuilder.CreateIndex(
                name: "project_id_key",
                table: "project",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_instance_id_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_instance_id_user_id_idx",
                schema: "auth",
                table: "refresh_tokens",
                columns: new[] { "instance_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_parent_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "parent");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_session_id_revoked_idx",
                schema: "auth",
                table: "refresh_tokens",
                columns: new[] { "session_id", "revoked" });

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_token_unique",
                schema: "auth",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_updated_at_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "updated_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_multipart_uploads_list",
                schema: "storage",
                table: "s3_multipart_uploads",
                columns: new[] { "bucket_id", "key", "created_at" })
                .Annotation("Relational:Collation", new[] { null, "C", null });

            migrationBuilder.CreateIndex(
                name: "IX_s3_multipart_uploads_parts_bucket_id",
                schema: "storage",
                table: "s3_multipart_uploads_parts",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_s3_multipart_uploads_parts_upload_id",
                schema: "storage",
                table: "s3_multipart_uploads_parts",
                column: "upload_id");

            migrationBuilder.CreateIndex(
                name: "saml_providers_entity_id_key",
                schema: "auth",
                table: "saml_providers",
                column: "entity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "saml_providers_sso_provider_id_idx",
                schema: "auth",
                table: "saml_providers",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_saml_relay_states_flow_state_id",
                schema: "auth",
                table: "saml_relay_states",
                column: "flow_state_id");

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_created_at_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_for_email_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "for_email");

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_sso_provider_id_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "sessions_not_after_idx",
                schema: "auth",
                table: "sessions",
                column: "not_after",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "sessions_user_id_idx",
                schema: "auth",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_id_created_at_idx",
                schema: "auth",
                table: "sessions",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "sso_domains_sso_provider_id_idx",
                schema: "auth",
                table: "sso_domains",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "user_mail_key",
                table: "user",
                column: "mail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "confirmation_token_idx",
                schema: "auth",
                table: "users",
                column: "confirmation_token",
                unique: true,
                filter: "((confirmation_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "email_change_token_current_idx",
                schema: "auth",
                table: "users",
                column: "email_change_token_current",
                unique: true,
                filter: "((email_change_token_current)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "email_change_token_new_idx",
                schema: "auth",
                table: "users",
                column: "email_change_token_new",
                unique: true,
                filter: "((email_change_token_new)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "reauthentication_token_idx",
                schema: "auth",
                table: "users",
                column: "reauthentication_token",
                unique: true,
                filter: "((reauthentication_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "recovery_token_idx",
                schema: "auth",
                table: "users",
                column: "recovery_token",
                unique: true,
                filter: "((recovery_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "users_email_partial_key",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true,
                filter: "(is_sso_user = false)");

            migrationBuilder.CreateIndex(
                name: "users_instance_id_idx",
                schema: "auth",
                table: "users",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "users_is_anonymous_idx",
                schema: "auth",
                table: "users",
                column: "is_anonymous");

            migrationBuilder.CreateIndex(
                name: "users_phone_key",
                schema: "auth",
                table: "users",
                column: "phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_entries",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Endpoint");

            migrationBuilder.DropTable(
                name: "identities",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "instances",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "realtime");

            migrationBuilder.DropTable(
                name: "mfa_amr_claims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "mfa_challenges",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "migrations",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "objects",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "one_time_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "s3_multipart_uploads_parts",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "saml_providers",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "saml_relay_states",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "schema_migrations",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "schema_migrations",
                schema: "realtime");

            migrationBuilder.DropTable(
                name: "sso_domains",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "subscription",
                schema: "realtime");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "mfa_factors",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "s3_multipart_uploads",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "flow_state",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sso_providers",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Database");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "buckets",
                schema: "storage");

            migrationBuilder.DropSequence(
                name: "seq_schema_version",
                schema: "graphql");
        }
    }
}
