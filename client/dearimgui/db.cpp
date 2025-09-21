#include "db.h"
#include <stdexcept>

namespace Taskpiea {

	Database::Database(const std::string& db_path) {
		if (sqlite3_open(db_path.c_str(), &DBHandle) != SQLITE_OK) {
			throw std::runtime_error("Failed to open database: " + std::string(sqlite3_errmsg(DBHandle)));
		}
		InitializeSchema();
	}

	Database::~Database() {
		sqlite3_close(DBHandle);
	}

	void Database::InitializeSchema() {
		const char* user_sql = "CREATE TABLE IF NOT EXISTS users ("
			"id INTEGER PRIMARY KEY, "
			"name TEXT NOT NULL UNIQUE);"; // Added UNIQUE constraint
		const char* task_sql = "CREATE TABLE IF NOT EXISTS tasks ("
			"id INTEGER PRIMARY KEY, "
			"name TEXT NOT NULL, "
			"description TEXT, "
			"status TEXT, "
			"assignee INTEGER, "
			"last_updated INTEGER, "
			"archived INTEGER, "
			"FOREIGN KEY (assignee) REFERENCES users(id));";

		char* err_msg = nullptr;
		if (sqlite3_exec(DBHandle, user_sql, nullptr, nullptr, &err_msg) != SQLITE_OK ||
			sqlite3_exec(DBHandle, task_sql, nullptr, nullptr, &err_msg) != SQLITE_OK) {
			std::string error = err_msg ? err_msg : "Unknown error";
			sqlite3_free(err_msg);
			throw std::runtime_error("Schema creation failed: " + error);
		}
	}

	bool Database::IsUserNameUnique(const std::string& name, unsigned int exclude_id) {
		const char* sql = "SELECT id FROM users WHERE name = ? AND id != ?;";
		sqlite3_stmt* stmt;
		if (sqlite3_prepare_v2(DBHandle, sql, -1, &stmt, nullptr) != SQLITE_OK) {
			return false;
		}
		sqlite3_bind_text(stmt, 1, name.c_str(), -1, SQLITE_STATIC);
		sqlite3_bind_int(stmt, 2, exclude_id);
		bool is_unique = sqlite3_step(stmt) != SQLITE_ROW;
		sqlite3_finalize(stmt);
		return is_unique;
	}

	ValidationResult Database::CreateUser(const User& user) {
		ValidationResult result = user.Validate();
		if (!result.IsSuccess) return result;

		if (!IsUserNameUnique(user.name)) {
			result.AddError("Name", "User name must be unique");
			return result;
		}

		const char* sql = "INSERT INTO users (id, name) VALUES (?, ?);";
		sqlite3_stmt* stmt;
		if (sqlite3_prepare_v2(DBHandle, sql, -1, &stmt, nullptr) != SQLITE_OK) {
			return ValidationResult::Failure("Failed to prepare SQL statement");
		}

		sqlite3_bind_int(stmt, 1, user.id);
		sqlite3_bind_text(stmt, 2, user.name.c_str(), -1, SQLITE_STATIC);
		if (sqlite3_step(stmt) != SQLITE_DONE) {
			sqlite3_finalize(stmt);
			return ValidationResult::Failure("Failed to create user: " + std::string(sqlite3_errmsg(DBHandle)));
		}
		sqlite3_finalize(stmt);
		return ValidationResult::Success();
	}

	ValidationResult Database::UpdateUser(const User& user) {
		ValidationResult result = user.Validate();
		if (!result.IsSuccess) return result;

		if (!IsUserNameUnique(user.name, user.id)) {
			result.AddError("Name", "User name must be unique");
			return result;
		}

		const char* sql = "UPDATE users SET name = ? WHERE id = ?;";
		sqlite3_stmt* stmt;
		if (sqlite3_prepare_v2(DBHandle, sql, -1, &stmt, nullptr) != SQLITE_OK) {
			return ValidationResult::Failure("Failed to prepare SQL statement");
		}

		sqlite3_bind_text(stmt, 1, user.name.c_str(), -1, SQLITE_STATIC);
		sqlite3_bind_int(stmt, 2, user.id);
		if (sqlite3_step(stmt) != SQLITE_DONE) {
			sqlite3_finalize(stmt);
			return ValidationResult::Failure("Failed to update user: " + std::string(sqlite3_errmsg(DBHandle)));
		}
		sqlite3_finalize(stmt);
		return ValidationResult::Success();
	}

	// Implement CreateTask, UpdateTask similarly, adding any task-specific validations
	// Other CRUD methods (ReadUser, DeleteUser, ListUsers, etc.) remain as previously defined
} // namespace Taskpiea