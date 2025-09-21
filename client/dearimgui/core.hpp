#pragma once
#include <string>
#include <cstdint>
#include <vector>

namespace Taskpiea {
	
	// ***********************************************************************
	// TODO: validation is not finished. Want to see how I would handle it on UI. 
	// Perhaps I would prefer to validate one field at a time instead?
	// It is also possible I dont need fields at all, and its just an 
	// entire object, on validation error at atime.

	struct FieldError {
		std::string field;
		std::string message;
	};

	struct ValidationResult {
		bool isSuccess;
		std::string errorMessage;
		std::vector<FieldError> fieldErrors;

		ValidationResult() : isSuccess(false) {}
		ValidationResult(bool isSuccess) : isSuccess(isSuccess) {}
		ValidationResult(const std::string errorMessage) : isSuccess(false), errorMessage(errorMessage) {}

		static ValidationResult Success() { return ValidationResult(true); }
		static ValidationResult Failure(const std::string errorMessage) { return ValidationResult(errorMessage); }

		void AddError(const std::string field, const std::string message) {
			fieldErrors.push_back({ field, message });
			isSuccess = false;
		}
	};
	// ***********************************************************************

	struct Project {
		std::string name;
	};

	enum UserFields { USER_FIELD_ID, USER_FIELD_NAME, USER_FIELD_LAST_UPDATED, USER_FIELD_ARCHIVED };

	struct User {
		unsigned int id; // min value = 1
		std::string name;
		std::int64_t lastUpdated;
		bool archived;
		
		User() : id(0), name(""), lastUpdated(0), archived(false) {}
		User(const User& other) : id(other.id), name(other.name), lastUpdated(other.lastUpdated), archived(other.archived) {}
	};

	enum TaskFileds { TASK_FIELD_ID, TASK_FIELD_NAME, TASK_FIELD_DESCRPTION, TASK_FIELD_STATUS, TASK_FIELD_ASSIGNEE, TASK_FIELD_LAST_UPDATED, TASK_FIELD_ARCHIVED };
	enum class TaskStatus { OPEN, IN_PROGRESS, VERIFY, DONE };

	struct Task {
		unsigned int id;
		std::string name;
		std::string description;
		TaskStatus status;
		unsigned int assigneeId; // links to User.Id
		std::int64_t lastUpdated;
		bool archived;
	};

	class DataCache {
	public:
		Project project;
		std::vector<User> users;
		std::vector<Task> tasks;

		void Clear();

		bool ArchiveUser(unsigned int id);
		bool CreateUser(User user);
		User GetUser(unsigned int id);
		bool UpdateUser(const User& user);
		ValidationResult ValidateUser(const User& user);

		bool ArchiveTask(unsigned int id);
		bool CreateTask(Task task);
		Task GetTask(unsigned int id);
		bool UpdateTask(const Task& task);
		ValidationResult ValidateTask(const Task& task);
	};

	enum class UIScreen { HOME_SCREEN, TASKS_SCREEN };

	class HomeControl {
	public:
		void CreateUI();
	};

	class UsersPopupControl {
	public:
		User editingUser;
		bool focusFlag = false;
		bool visible;
		void Show() { visible = true; }
		void Hide() { visible = false; }
	};

	class TasksControl {
	};

	class TaskPopupControl {
	public:
		Task* editingTask;
		bool visible;
		TaskPopupControl(Task* task) : editingTask(task), visible(true) {}
		void Show() { visible = true; }
		void Hide() { visible = false; }
	};

	class AboutPopupControl {
	public:
		bool visible;
		void Show() { visible = true; }
		void Hide() { visible = false; }
	};

	struct UIContext {
		UIScreen currentScreen;
		HomeControl homeControl;
		UsersPopupControl usersPopupControl;
		TasksControl tasksControl;
		std::vector<TaskPopupControl> taskPopupControls;
		AboutPopupControl aboutPopupControl;
	};

	class App {
	public:
		DataCache dataCache;
		UIContext uiContext;
		void CreateUI();
	private:
		void CreateDummyData();

		void CreateMainMenu();
		void CreateUsersPopupControl();
		void CreateTasksControl();
		void CreateTaskPopupControl(TaskPopupControl& control);
		void CreateAboutControl();

		void AddTaskPopup(Task& task);
		
		void CreateProject();
		void OpenProject();
		void CloseProject();
		void SaveProject();
	};
}