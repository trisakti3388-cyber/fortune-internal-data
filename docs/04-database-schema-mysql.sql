-- Fortune Internal Data
-- MySQL 8+ schema for MVP

CREATE DATABASE IF NOT EXISTS fortune_internal_data
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE fortune_internal_data;

CREATE TABLE users (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL,
    two_factor_secret VARCHAR(255) NULL,
    two_factor_enabled TINYINT(1) NOT NULL DEFAULT 0,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_users_email (email),
    KEY idx_users_role (role),
    KEY idx_users_is_active (is_active)
) ENGINE=InnoDB;

CREATE TABLE phone_numbers (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    seq VARCHAR(50) NULL,
    phone_number VARCHAR(14) NOT NULL,
    remark TEXT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    whatsapp_status VARCHAR(20) NULL,
    upload_date DATETIME NULL,
    modified_date DATETIME NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_phone_numbers_phone_number (phone_number),
    KEY idx_phone_numbers_status (status),
    KEY idx_phone_numbers_whatsapp_status (whatsapp_status),
    KEY idx_phone_numbers_upload_date (upload_date),
    KEY idx_phone_numbers_modified_date (modified_date)
) ENGINE=InnoDB;

CREATE TABLE import_batches (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    file_name VARCHAR(255) NOT NULL,
    stored_file_path VARCHAR(255) NULL,
    uploaded_by_user_id BIGINT UNSIGNED NOT NULL,
    total_rows INT NOT NULL DEFAULT 0,
    new_rows INT NOT NULL DEFAULT 0,
    existing_rows INT NOT NULL DEFAULT 0,
    invalid_rows INT NOT NULL DEFAULT 0,
    duplicate_rows INT NOT NULL DEFAULT 0,
    status VARCHAR(30) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY idx_import_batches_uploaded_by_user_id (uploaded_by_user_id),
    KEY idx_import_batches_status (status),
    KEY idx_import_batches_created_at (created_at),
    CONSTRAINT fk_import_batches_uploaded_by_user
        FOREIGN KEY (uploaded_by_user_id) REFERENCES users(id)
        ON UPDATE RESTRICT ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE import_batch_rows (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    batch_id BIGINT UNSIGNED NOT NULL,
    seq VARCHAR(50) NULL,
    raw_phone_number VARCHAR(100) NULL,
    normalized_phone_number VARCHAR(14) NULL,
    remark TEXT NULL,
    row_status VARCHAR(30) NOT NULL,
    message VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY idx_import_batch_rows_batch_id (batch_id),
    KEY idx_import_batch_rows_row_status (row_status),
    KEY idx_import_batch_rows_normalized_phone_number (normalized_phone_number),
    CONSTRAINT fk_import_batch_rows_batch
        FOREIGN KEY (batch_id) REFERENCES import_batches(id)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE activity_logs (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    user_id BIGINT UNSIGNED NOT NULL,
    action VARCHAR(50) NOT NULL,
    target_type VARCHAR(50) NOT NULL,
    target_id BIGINT UNSIGNED NOT NULL,
    old_value_json JSON NULL,
    new_value_json JSON NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY idx_activity_logs_user_id (user_id),
    KEY idx_activity_logs_action (action),
    KEY idx_activity_logs_target (target_type, target_id),
    KEY idx_activity_logs_created_at (created_at),
    CONSTRAINT fk_activity_logs_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON UPDATE RESTRICT ON DELETE RESTRICT
) ENGINE=InnoDB;

-- Optional seed note:
-- Insert the first Superadmin user through application bootstrap or a secure migration step.

-- Recommended application-level validation:
-- phone_number must match regex ^[0-9]{10,14}$
-- status must be one of: active, inactive
-- whatsapp_status must be one of: 1day, 3day, 7day, active, inactive
-- role must be one of: Superadmin, Admin, Manager, Staff
