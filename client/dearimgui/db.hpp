#pragma once
#include "core.hpp"
#include <sqlite/sqlite3.h>
#include <vector>
#include <optional>

namespace Taskpiea {

	struct ValidationError {
		std::string Field;
		std::string Message;
	};

	struct ValidationResult {
		bool IsSuccess;
		std::string ErrorMessage;
		std::vector<ValidationError> Errors;

		ValidationResult() IsSuccess(false) {}
		ValidationResult(bool isSuccess) : IsSuccess(isSuccess) {}
		ValidationResult(const std::string errorMessage) IsSuccess(false), ErrorMessage(errorMessage) {}

		static ValidationResult Success() { return ValidationResult(true); }
		static ValidationResult Failure(const std::string errorMessage) { return ValidationResult(errorMessage); }

		void AddError(const std::string field, const std::string message) {
			Errors.push_back({field, message});
			IsSuccess = false;
		}
	};

	class Database {
	public:
		Database(const std::string& db_path);
		~Database();

		// User CRUD
		bool CreateUser(const User& user);
		std::optional<User> ReadUser(unsigned int id);
		bool UpdateUser(const User& user);
		bool DeleteUser(unsigned int id);
		std::vector<User> ListUsers();

		// Task CRUD
		bool CreateTask(const Task& task);
		std::optional<Task> ReadTask(unsigned int id);
		bool UpdateTask(const Task& task);
		bool DeleteTask(unsigned int id);
		std::vector<Task> ListTasks();

	private:
		sqlite3* DBHandle;
		void InitializeSchema();
	};
}