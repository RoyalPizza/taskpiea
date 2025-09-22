#include "core.hpp"

#include "imgui/imgui.h"
#include "imgui/imgui_stdlib.h"
#include <string>

#ifdef _WIN32
#include <windows.h>
#include <commdlg.h> // For OPENFILENAME and GetOpenFileName
#endif

using namespace Taskpiea;

static const size_t TASK_STATUS_COUNT = 4;
static const TaskStatus TaskStatusArray[] = { TaskStatus::OPEN, TaskStatus::IN_PROGRESS, TaskStatus::VERIFY, TaskStatus::DONE };
static const char* TaskStatusNameArray[] = { "OPEN", "IN PROGRESS", "VERIFY", "DONE" };
static const ImVec4 StatusColors[] = {
	{0.5f, 0.5f, 0.5f, 1.0f}, // OPEN: Gray
	{0.5f, 0.0f, 1.0f, 1.0f}, // IN_PROGRESS: Purple
	{1.0f, 1.0f, 0.0f, 1.0f}, // VERIFY: Yellow
	{0.0f, 1.0f, 0.0f, 1.0f}  // DONE: Green
};

const char* GetTaskStatusName(TaskStatus status) {
	size_t index = static_cast<size_t>(status);
	if (index < TASK_STATUS_COUNT) {
		return TaskStatusNameArray[index];
	}
	return "UNKNOWN";
}

ImVec4 GetTaskStatusColor(TaskStatus status) {
	size_t index = static_cast<size_t>(status);
	if (index < TASK_STATUS_COUNT) {
		return StatusColors[index];
	}
	return ImVec4(1.0f, 1.0f, 1.0f, 1.0f); // White fallback
}

// TODO: Change this to be a map instead

void DataCache::Clear() {
	project.name = "";
	users.clear();
	tasks.clear();
}

bool DataCache::ArchiveUser(unsigned int id) {
	for (size_t i = 0; i < tasks.size(); i++) {
		if (tasks[i].assigneeId == id) {
			tasks[i].assigneeId = 0;
		}
	}
	
	for (size_t i = 0; i < users.size(); i++) {
		if (users[i].id == id) {
			users[i].archived = true;
			return true;
		}
	}

	return false;
}

bool DataCache::CreateUser(User user) {
	ValidationResult result = ValidateUser(user);
	if (result.isSuccess) {
		user.id = users.empty() ? 1 : users.back().id + 1;
		users.push_back(user);
		return true;
	}
	return false;
}

User DataCache::GetUser(unsigned int id) {
	for (size_t i = 0; i < users.size(); i++) {
		if (users[i].id == id)
			return users[i];
	}
	return User();
}

bool DataCache::UpdateUser(const User& user) {
	ValidationResult result = ValidateUser(user);
	if (result.isSuccess) {
		for (size_t i = 0; i < users.size(); i++) {
			if (users[i].id == user.id) {
				users[i] = user;
				return true;
			}
		}
	}
	return false;
}

ValidationResult DataCache::ValidateUser(const User& user) {
	if (user.name.empty()) {
		return ValidationResult::Failure("name is empty");
	}
	for (size_t i = 0; i < users.size(); i++) {
		if (users[i].id != user.id && users[i].name == user.name) {
			return ValidationResult::Failure("name field must be unique");
		}
	}
	return ValidationResult::Success();
}

bool DataCache::ArchiveTask(unsigned int id) {
	for (size_t i = 0; i < tasks.size(); i++) {
		if (tasks[i].id == id) {
			tasks[i].archived = true;
			return true;
		}
	}
	return false;
}

bool DataCache::CreateTask(Task task) {
	ValidationResult result = ValidateTask(task);
	if (result.isSuccess) {
		task.id = tasks.empty() ? 1 : tasks.back().id + 1;
		tasks.push_back(task);
		return true;
	}
	return false;
}

Task DataCache::GetTask(unsigned int id) {
	for (size_t i = 0; i < tasks.size(); i++) {
		if (tasks[i].id == id)
			return tasks[i];
	}
	return Task();
}

bool DataCache::UpdateTask(const Task& task) {
	ValidationResult result = ValidateTask(task);
	if (result.isSuccess) {
		for (size_t i = 0; i < tasks.size(); i++) {
			if (tasks[i].id == task.id) {
				tasks[i] = task;
				return true;
			}
		}
	}
	return false;
}

ValidationResult DataCache::ValidateTask(const Task& task) {
	// as of right now, there is nothing to validate for tasks
	return ValidationResult::Success();
}

// -----------------------------------------------------------
// -----------------------------------------------------------
// -----------------------------------------------------------
//                    App
// -----------------------------------------------------------
// -----------------------------------------------------------
// -----------------------------------------------------------



void App::CreateUI() {
	CreateMainMenu();
	static ImGuiWindowFlags flags = ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_NoMove
		| ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_NoBringToFrontOnFocus;
	ImGuiViewport* viewport = ImGui::GetMainViewport();
	ImGui::SetNextWindowPos(viewport->WorkPos);
	ImGui::SetNextWindowSize(viewport->WorkSize);
	ImGui::Begin("Main", nullptr, flags);
	switch (uiContext.currentScreen) {
		case UIScreen::HOME_SCREEN:
			break;
		case UIScreen::TASKS_SCREEN:
			CreateTasksControl();
			break;
		default:
			break;
	};
	if (uiContext.usersPopupControl.visible) {
		CreateUsersPopupControl();
	}
	if (uiContext.aboutPopupControl.visible) {
		CreateAboutControl();
	}

	std::vector<size_t> controlsToRemove;
	for (size_t i = 0; i < uiContext.taskPopupControls.size(); i++) {
		TaskPopupControl& control = uiContext.taskPopupControls.at(i);
		if (control.visible) {
			CreateTaskPopupControl(control);
		} else {
			controlsToRemove.push_back(i);
		}
	}
	// loop through controlsToRemove and remove them from uiContext.taskPopupControls
	while (controlsToRemove.size() > 0) {
		size_t indexToRemove = controlsToRemove.back();
		uiContext.taskPopupControls.at(indexToRemove) = uiContext.taskPopupControls.back();
		uiContext.taskPopupControls.pop_back();
		//uiContext.taskPopupControls.erase(uiContext.taskPopupControls.begin() + indexToRemove);
		controlsToRemove.pop_back();
	}

	ImGui::End();
}

void App::CreateMainMenu() {
	bool projectIsOpen = dataCache.project.name != "";

	if (ImGui::BeginMainMenuBar())
	{
		if (ImGui::BeginMenu("File")) {
			if (ImGui::MenuItem("Create Project")) { CreateProject(); }
			if (ImGui::MenuItem("Open Project", "Ctrl+O")) { OpenProject(); }
			if (ImGui::MenuItem("Close Project", nullptr, false, projectIsOpen)) { CloseProject(); }
			if (ImGui::MenuItem("Save Project", "Ctrl+S", false, projectIsOpen)) { SaveProject(); }
			ImGui::Separator();
			if (ImGui::MenuItem("Exit", "Alt+F4")) { }
			ImGui::EndMenu();
		}
		if (ImGui::BeginMenu("Project")) {
			//(ImGui::MenuItem("Edit Tasks");
			if (ImGui::MenuItem("Edit Users", nullptr, false, projectIsOpen)) { uiContext.usersPopupControl.Show(); }
			ImGui::EndMenu();
		}
		if (ImGui::BeginMenu("Help")) {
			if (ImGui::MenuItem("About")) { uiContext.aboutPopupControl.Show(); }
			ImGui::EndMenu();
		}
		ImGui::EndMainMenuBar();
	}
}

void App::CreateUsersPopupControl() {
	ImGui::Begin("UsersControl", &uiContext.usersPopupControl.visible);
	static ImGuiTableFlags flags = ImGuiTableFlags_RowBg | ImGuiTableFlags_BordersOuter | ImGuiTableFlags_BordersV
		| ImGuiTableFlags_NoBordersInBody | ImGuiTableFlags_ScrollY;

	if (ImGui::BeginTable("Users", 2, flags, ImVec2(0.0f, ImGui::GetContentRegionAvail().y), 0.0f)) {
		ImGui::TableSetupColumn("ID", ImGuiTableColumnFlags_WidthFixed, 0.0f, USER_FIELD_ID);
		ImGui::TableSetupColumn("Name", ImGuiTableColumnFlags_WidthFixed, 0.0f, USER_FIELD_NAME);
		ImGui::TableSetupScrollFreeze(0, 1); // Make row always visible
		ImGui::TableHeadersRow();

		ImGuiListClipper clipper;
		clipper.Begin(dataCache.users.size());
		while (clipper.Step()) {
			for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
				User& user = (dataCache.users.at(i).id == uiContext.usersPopupControl.editingUser.id)
					? uiContext.usersPopupControl.editingUser
					: dataCache.users.at(i);

				ImGui::PushID(user.id);
				ImGui::TableNextRow();
				ImGui::TableNextColumn();
				ImGui::Text("%04d", user.id);
				ImGui::TableNextColumn();

				// - one row/field to be edited at a time
				// - allow right click copy (no ctrl c support atm)
				// - auto focus the first frame after showing text box
				// - red border on validate fail (does not work because textbox text is cached by imgui, not on my object)
				// - pres escape at any time to cancel

				if (uiContext.usersPopupControl.editingUser.id == user.id) {
					char buffer[256];
					strncpy_s(buffer, user.name.c_str(), sizeof(buffer));
					ImGui::SetNextItemWidth(200.0f); // Set width to 200 pixels
					if (uiContext.usersPopupControl.focusFlag) {
						ImGui::SetKeyboardFocusHere();
						uiContext.usersPopupControl.focusFlag = false;
					}
					auto result = dataCache.ValidateUser(user);
					if (result.isSuccess == false) {
						ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f); // Ensure border is visible
						ImGui::PushStyleColor(ImGuiCol_Border, ImVec4(1.0f, 0.0f, 0.0f, 1.0f)); // Set red border
					}
					if (ImGui::InputText("##Name", buffer, sizeof(buffer), ImGuiInputTextFlags_EnterReturnsTrue)) {
						user.name = buffer;
						dataCache.UpdateUser(user);
						uiContext.usersPopupControl.editingUser = User();
					} else {
						user.name = buffer;
					}
					if (ImGui::IsKeyPressed(ImGuiKey_Escape)) {
						uiContext.usersPopupControl.editingUser = User();
					}
					if (result.isSuccess == false) {
						ImGui::PopStyleColor(); // Reset border color
						ImGui::PopStyleVar(); // Reset border size
					}
				}
				else if (uiContext.usersPopupControl.editingUser.id == 0) {
					if (ImGui::Selectable(user.name.c_str(), false, ImGuiSelectableFlags_AllowDoubleClick | ImGuiSelectableFlags_AllowItemOverlap)) {
						if (ImGui::IsMouseDoubleClicked(0)) {
							uiContext.usersPopupControl.editingUser = User(user);
							uiContext.usersPopupControl.focusFlag = true;
						}
					}
					if (ImGui::BeginPopupContextItem("context_menu")) {
						if (ImGui::Selectable("Copy")) {
							ImGui::SetClipboardText(user.name.c_str());
						}
						ImGui::EndPopup();
					}
				}
				else {
					ImGui::Selectable(user.name.c_str(), false, ImGuiSelectableFlags_AllowItemOverlap);
					if (ImGui::BeginPopupContextItem("context_menu")) {
						if (ImGui::Selectable("Copy")) {
							ImGui::SetClipboardText(user.name.c_str());
						}
						ImGui::EndPopup();
					}
				}

				ImGui::PopID();
			}
		}

		ImGui::EndTable();
	}
	
	ImGui::End();
}

void App::CreateTasksControl() {
	static ImGuiTableFlags flags = ImGuiTableFlags_RowBg | ImGuiTableFlags_BordersOuter | ImGuiTableFlags_BordersV
		| ImGuiTableFlags_NoBordersInBody | ImGuiTableFlags_ScrollY;

	if (ImGui::BeginTable("Tasks", 5, flags, ImVec2(0.0f, ImGui::GetContentRegionAvail().y), 0.0f)) {
		ImGui::TableSetupColumn("ID", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_ID);
		ImGui::TableSetupColumn("Name", ImGuiTableColumnFlags_WidthFixed, 200.0f, TASK_FIELD_NAME);
		ImGui::TableSetupColumn("Description", ImGuiTableColumnFlags_WidthFixed, 500.0f, TASK_FIELD_DESCRPTION);
		ImGui::TableSetupColumn("Status", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_STATUS);
		ImGui::TableSetupColumn("Assignee", ImGuiTableColumnFlags_WidthFixed, 150.0f, TASK_FIELD_ASSIGNEE);
		ImGui::TableSetupScrollFreeze(0, 1); // Make row always visible
		ImGui::TableHeadersRow();

		ImGuiListClipper clipper;
		clipper.Begin(dataCache.tasks.size());
		while (clipper.Step()) {
			for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
				Task& task = dataCache.tasks.at(i);
				ImGui::PushID(task.id);
				ImGui::TableNextRow();
				ImGui::TableNextColumn();
				
				char buffer[16];
				snprintf(buffer, sizeof(buffer), "%04d", task.id);
				const char* taskId = buffer;
				float rowHeight = ImGui::GetFrameHeight();
				if (ImGui::Selectable(taskId, false, ImGuiSelectableFlags_SpanAllColumns | ImGuiSelectableFlags_AllowOverlap | ImGuiSelectableFlags_AllowDoubleClick, ImVec2(0, rowHeight))) {
					if (ImGui::IsMouseDoubleClicked(0)) {
						AddTaskPopup(task);
					}
				}

				ImGui::TableNextColumn();
				ImGui::TextUnformatted(task.name.c_str());
				ImGui::TableNextColumn();
				ImGui::TextUnformatted(task.description.c_str());
				ImGui::TableNextColumn();

				// status requires no verification, always allow it to change
				const char* statusPreview = GetTaskStatusName(task.status);
				ImGui::SetNextItemWidth(120.0f);
				ImGui::PushStyleColor(ImGuiCol_Text, GetTaskStatusColor(task.status));
				ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0.0f, 0.0f, 0.0f, 0.0f));
				if (ImGui::BeginCombo("##Status", statusPreview, ImGuiComboFlags_HeightSmall)) {
					for (size_t i = 0; i < TASK_STATUS_COUNT; ++i) {
						bool isSelected = task.status == TaskStatusArray[i];
						ImGui::PushStyleColor(ImGuiCol_Text, GetTaskStatusColor(TaskStatusArray[i]));
						if (ImGui::Selectable(TaskStatusNameArray[i], isSelected)) {
							task.status = TaskStatusArray[i];;
						}
						if (isSelected) {
							ImGui::SetItemDefaultFocus();
						}
						ImGui::PopStyleColor();
					}

					ImGui::EndCombo();
				}
				ImGui::PopStyleColor(2);				
				ImGui::TableNextColumn();

				User currentUser = dataCache.GetUser(task.assigneeId);
				const char* assigneePreview = (task.assigneeId != 0) ? currentUser.name.c_str() : "None";
				ImGui::SetNextItemWidth(120.0f);
				if (ImGui::BeginCombo("##Assignee", assigneePreview, ImGuiComboFlags_HeightLarge)) {
					if (ImGui::Selectable("None", task.assigneeId == 0)) {
						task.assigneeId = 0;
					}
					for (size_t i = 0; i < dataCache.users.size(); i++) {
						User& user = dataCache.users.at(i);
						bool isSelected = task.assigneeId == user.id;
						if (ImGui::Selectable(user.name.c_str(), isSelected)) {
							task.assigneeId = user.id;
						}
						if (isSelected) {
							ImGui::SetItemDefaultFocus();
						}
					}
					ImGui::EndCombo();
				}

				if (ImGui::IsItemHovered() && ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Left)) {
					AddTaskPopup(task);
				}

				ImGui::PopID();
			}
		}
	}

	ImGui::EndTable();
}

void App::CreateTaskPopupControl(TaskPopupControl& control) {
	std::string label = "#" + std::to_string(control.editingTask->id);
	ImGui::SetNextWindowSize(ImVec2(400, 500), ImGuiCond_FirstUseEver);
	ImGui::Begin(label.c_str(), &control.visible, ImGuiWindowFlags_NoSavedSettings);

	Task* task = control.editingTask;

	if (ImGui::BeginTable("##properties", 2, ImGuiTableFlags_ScrollY)) {
		ImGui::TableSetupColumn("", ImGuiTableColumnFlags_WidthFixed);
		ImGui::TableSetupColumn("", ImGuiTableColumnFlags_WidthStretch, 2.0f);

		ImGui::TableNextRow();
		ImGui::PushID("name");
		ImGui::TableNextColumn();
		ImGui::AlignTextToFramePadding();
		ImGui::TextUnformatted("name");
		ImGui::TableNextColumn();
		char nameBuffer[256];
		strncpy_s(nameBuffer, task->name.c_str(), sizeof(nameBuffer));
		// ImVec2(-FLT_MIN, ImGui::GetTextLineHeight()); I use this on my multi line, can I use it on normal line?
		//ImGui::InputText("##name", nameBuffer, 28);
		ImGui::SetNextItemWidth(-FLT_MIN); // full width of current column/window
		//ImGui::InputText("##name", nameBuffer, 28);
		ImGui::InputText("##name", &task->name);

		ImGui::PopID();

		ImGui::TableNextRow();
		ImGui::PushID("description");
		ImGui::TableNextColumn();
		ImGui::AlignTextToFramePadding();
		ImGui::TextUnformatted("description");
		ImGui::TableNextColumn();
		ImGuiInputTextFlags flags = ImGuiInputTextFlags_AllowTabInput | ImGuiInputTextFlags_WordWrap | ImGuiInputTextFlags_CtrlEnterForNewLine;
		if (ImGui::InputTextMultiline("##description", &task->description, ImVec2(-FLT_MIN, ImGui::GetTextLineHeight() * 16), flags)) {
			
		}
		ImGui::PopID();

		ImGui::TableNextRow();
		ImGui::PushID("status");
		ImGui::TableNextColumn();
		ImGui::AlignTextToFramePadding();
		ImGui::TextUnformatted("status");
		ImGui::TableNextColumn();
		const char* statusPreview = GetTaskStatusName(task->status);
		ImGui::SetNextItemWidth(150.0f);
		ImGui::PushStyleColor(ImGuiCol_Text, GetTaskStatusColor(task->status));
		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0.0f, 0.0f, 0.0f, 0.0f));
		if (ImGui::BeginCombo("##Status", statusPreview, ImGuiComboFlags_HeightSmall)) {
			for (size_t i = 0; i < TASK_STATUS_COUNT; ++i) {
				bool isSelected = task->status == TaskStatusArray[i];
				ImGui::PushStyleColor(ImGuiCol_Text, GetTaskStatusColor(TaskStatusArray[i]));
				if (ImGui::Selectable(TaskStatusNameArray[i], isSelected)) {
					task->status = TaskStatusArray[i];;
				}
				if (isSelected) {
					ImGui::SetItemDefaultFocus();
				}
				ImGui::PopStyleColor();
			}

			ImGui::EndCombo();
		}
		ImGui::PopStyleColor(2);				
		ImGui::PopID();

		ImGui::TableNextRow();
		ImGui::PushID("assignee");
		ImGui::TableNextColumn();
		ImGui::AlignTextToFramePadding();
		ImGui::TextUnformatted("assignee");
		ImGui::TableNextColumn();
		User currentUser = dataCache.GetUser(task->assigneeId);
		const char* assigneePreview = (task->assigneeId != 0) ? currentUser.name.c_str() : "None";
		ImGui::SetNextItemWidth(150.0f);
		if (ImGui::BeginCombo("##Assignee", assigneePreview, ImGuiComboFlags_HeightLarge)) {
			if (ImGui::Selectable("None", task->assigneeId == 0)) {
				task->assigneeId = 0;
			}
			for (size_t i = 0; i < dataCache.users.size(); i++) {
				User& user = dataCache.users.at(i);
				bool isSelected = task->assigneeId == user.id;
				if (ImGui::Selectable(user.name.c_str(), isSelected)) {
					task->assigneeId = user.id;
				}
				if (isSelected) {
					ImGui::SetItemDefaultFocus();
				}
			}
			ImGui::EndCombo();
		}
		ImGui::PopID();

		ImGui::EndTable();
	}

	ImGui::End();
}

void App::AddTaskPopup(Task& task) {
	for (int i = 0; i < uiContext.taskPopupControls.size(); i++) {
		const TaskPopupControl& control = uiContext.taskPopupControls.at(i);
		if (control.editingTask->id == task.id) {
			return; // return now because one already exists
		}
	}

	uiContext.taskPopupControls.push_back(TaskPopupControl(&task));
}

void App::CreateAboutControl() {
    ImGui::Begin("AboutControl", &uiContext.aboutPopupControl.visible);

	// GOOFY AI GENERATED ASCII ART FOR THE HECK OF IT

    static float t = 0.0f;
    t += ImGui::GetIO().DeltaTime * 3.0f;
    int frame = (int)t % 2;

    // Colors
    ImVec4 crust   = ImVec4(0.9f, 0.7f, 0.4f, 1.0f);
    ImVec4 cheese  = ImVec4(1.0f, 1.0f, 0.2f, 1.0f);
    ImVec4 pep     = ImVec4(0.9f, 0.1f, 0.1f, 1.0f);
    ImVec4 olive   = ImVec4(0.1f, 0.6f, 0.1f, 1.0f);
    ImVec4 sparkle = ImVec4(1.0f, 0.8f, 0.8f, 1.0f);

    const char* pie[9] = {
        "      ______      ",
        "   .-'      `-.   ",
        " .'            `. ",
        "/   O   o   o    \\",
        "|  o   PIZZA!  o |",
        "\\    o   o   O   /",
        " `.            .' ",
        "   `-.______.-'   ",
        "                  "
    };

    for (int i = 0; i < 9; i++) {
        for (const char* c = pie[i]; *c; c++) {
            if (*c == 'O' || *c == 'o') {
                if (((c - pie[i]) + i + frame) % 2 == 0)
                    ImGui::SameLine(0,0), ImGui::TextColored(pep, "o");
                else
                    ImGui::SameLine(0,0), ImGui::TextColored(olive, "o");
            } else if (*c == '.' || *c == '-' || *c == '_' || *c == '`' || *c == '\'' || *c == '/') {
                ImGui::SameLine(0,0), ImGui::TextColored(crust, "%c", *c);
            } else if (*c == '*') {
                ImGui::SameLine(0,0), ImGui::TextColored(sparkle, "%c", frame ? '*' : ' ');
            } else {
                ImGui::SameLine(0,0), ImGui::TextColored(cheese, "%c", *c);
            }
        }
        // end of line → let ImGui break naturally
        ImGui::NewLine();
    }

    ImGui::End();
}

std::string OpenFileDialog() {
	OPENFILENAME ofn = { 0 };
	wchar_t szFile[260] = { 0 };
	ofn.lStructSize = sizeof(OPENFILENAME);
	ofn.lpstrFile = szFile;
	ofn.nMaxFile = sizeof(szFile) / sizeof(wchar_t);
	ofn.lpstrFilter = L"Taskp Files (*.taskp)\0*.taskp\0";
	ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;
	if (GetOpenFileName(&ofn)) {
		char buffer[260];
		size_t converted;
		wcstombs_s(&converted, buffer, szFile, sizeof(buffer));
		return std::string(buffer);
	}
	return "";
}

#include <random>
void App::CreateDummyData() {
	std::vector<std::string> names = {
        "Alice Smith", "Bob Johnson", "Charlie Brown", "Diana Wilson", "Emma Davis",
        "Frank Miller", "Grace Taylor", "Henry Anderson", "Isabella Thomas", "Jack White"
    };

	for (int i = 0; i < names.size(); i++) {
		User user;
		user.name = names.at(i);
		user.lastUpdated = 0;
		user.archived = false;

		dataCache.CreateUser(user);
	}

	std::vector<std::pair<std::string, std::string>> tasks = {
        {"Implement Login Module", "Create secure user authentication with OAuth2."},
        {"Optimize Database Queries", "Refactor SQL queries for better performance."},
        {"Design REST API", "Develop endpoints for user management system."},
        {"Fix Memory Leaks", "Identify and resolve memory leaks in backend."},
        {"Add Unit Tests", "Write unit tests for payment processing module."},
        {"Refactor Legacy Code", "Modernize old codebase to use C++20 features."},
        {"Integrate CI/CD Pipeline", "Set up Jenkins for automated builds."},
        {"Create UI Components", "Develop reusable React components for dashboard."},
        {"Enhance Error Handling", "Improve exception handling in core services."},
        {"Migrate to Cloud", "Move application to AWS infrastructure."},
        {"Add Logging Framework", "Implement structured logging with spdlog."},
        {"Optimize Frontend", "Reduce page load time for web app."},
        {"Secure API Endpoints", "Add JWT authentication to REST APIs."},
        {"Write Documentation", "Create API documentation using Swagger."},
        {"Improve Caching", "Implement Redis for session caching."},
        {"Fix UI Bugs", "Resolve alignment issues in mobile view."},
        {"Add Feature Flags", "Implement feature toggles for A/B testing."},
        {"Upgrade Dependencies", "Update third-party libraries to latest versions."},
        {"Automate Testing", "Set up Selenium for end-to-end tests."},
        {"Refactor Database Schema", "Normalize tables for better scalability."},
        {"Implement Rate Limiting", "Add rate limiting to prevent API abuse."},
        {"Add Monitoring", "Integrate Prometheus for system monitoring."},
        {"Optimize Build Process", "Reduce build time with parallel compilation."},
        {"Create CLI Tool", "Develop command-line tool for admin tasks."},
        {"Add Localization", "Support multiple languages in UI."},
        {"Fix Security Vulnerabilities", "Address XSS issues in frontend."},
        {"Implement WebSockets", "Add real-time updates to chat feature."},
        {"Refactor Authentication", "Switch to single sign-on system."},
        {"Add Backup System", "Implement automated database backups."},
        {"Optimize Images", "Compress images for faster page loads."},
        {"Create Admin Panel", "Build dashboard for user management."},
        {"Add Analytics", "Integrate Google Analytics for user tracking."},
        {"Fix Concurrency Issues", "Resolve race conditions in multithreaded code."},
        {"Implement Search", "Add Elasticsearch for full-text search."},
        {"Upgrade Framework", "Migrate to latest Spring Boot version."},
        {"Add Notifications", "Implement email notifications for user actions."},
        {"Refactor CSS", "Switch to Tailwind CSS for styling."},
        {"Improve Accessibility", "Ensure WCAG compliance for UI."},
        {"Add GraphQL API", "Implement GraphQL for flexible queries."},
        {"Optimize Docker Images", "Reduce container size for faster deployment."},
        {"Fix API Latency", "Improve response time for critical endpoints."},
        {"Add Load Balancer", "Set up Nginx for load balancing."},
        {"Implement Caching Layer", "Use Memcached for query caching."},
        {"Refactor Monolith", "Break down app into microservices."},
        {"Add Audit Logging", "Track user actions for compliance."},
        {"Fix Broken Links", "Resolve 404 errors in web app."},
        {"Implement OAuth", "Add Google login support."},
        {"Optimize SQL Joins", "Refactor complex joins for performance."},
        {"Add Dark Mode", "Implement dark theme for UI."},
        {"Fix Session Bugs", "Resolve session expiration issues."},
        {"Create API Client", "Build Python client for REST API."},
        {"Add Health Checks", "Implement endpoint for system status."},
        {"Refactor ORM", "Switch to SQLAlchemy for database access."},
        {"Improve Test Coverage", "Increase unit test coverage to 90%."},
        {"Add Rate Limiter", "Implement throttling for API requests."},
        {"Fix Memory Usage", "Optimize memory consumption in backend."},
        {"Add User Roles", "Implement role-based access control."},
        {"Optimize Frontend Build", "Reduce Webpack build time."},
        {"Add File Upload", "Support file uploads in web app."},
        {"Fix CORS Issues", "Resolve cross-origin request problems."},
        {"Implement Pagination", "Add pagination to API responses."},
        {"Add Metrics Dashboard", "Create Grafana dashboard for metrics."},
        {"Refactor Middleware", "Simplify Express middleware logic."},
        {"Add Email Templates", "Design HTML templates for emails."},
        {"Fix Data Validation", "Strengthen input validation in forms."},
        {"Implement Pub/Sub", "Add Redis Pub/Sub for event handling."},
        {"Optimize CDN Usage", "Configure Cloudflare for static assets."},
        {"Add API Versioning", "Support multiple API versions."},
        {"Fix Unit Test Failures", "Resolve flaky tests in CI."},
        {"Add Backup Encryption", "Encrypt database backups."},
        {"Refactor State Management", "Switch to Redux for state handling."},
        {"Add Rate Limiting Middleware", "Implement throttling in Express."},
        {"Fix API Documentation", "Update outdated API docs."},
        {"Add Session Management", "Implement secure session handling."},
        {"Optimize Query Performance", "Index tables for faster queries."},
        {"Add User Feedback", "Implement feedback form in UI."},
        {"Fix Mobile Bugs", "Resolve issues in responsive design."},
        {"Add Logging Levels", "Configure debug/info/error logging."},
        {"Implement Circuit Breaker", "Add resilience to API calls."},
        {"Add API Mocking", "Create mocks for API testing."},
        {"Fix Data Migration", "Resolve issues in database migration."},
        {"Add User Onboarding", "Create guided tour for new users."},
        {"Optimize Webpack Config", "Improve frontend build performance."},
        {"Add Error Tracking", "Integrate Sentry for error monitoring."},
        {"Fix Authentication Bugs", "Resolve issues in login flow."},
        {"Add Data Export", "Implement CSV export for reports."},
        {"Refactor API Routes", "Simplify routing structure."},
        {"Add Multi-Tenancy", "Support multiple clients in app."},
        {"Fix Performance Bottlenecks", "Optimize slow API endpoints."},
        {"Add Custom Dashboards", "Allow users to create dashboards."},
        {"Implement Retry Logic", "Add retries for failed API calls."},
        {"Fix UI Flickering", "Resolve rendering issues in React."},
        {"Add Data Validation", "Strengthen backend input validation."},
        {"Optimize Asset Delivery", "Use CDN for static assets."},
        {"Add User Permissions", "Implement fine-grained permissions."},
        {"Fix Memory Leaks in Tests", "Resolve leaks in test suite."},
        {"Add API Rate Limits", "Enforce limits on API usage."},
        {"Refactor Build Scripts", "Simplify CI/CD build process."},
        {"Add Offline Support", "Implement service workers for offline mode."}
    };

	std::vector<int> assigneeIds;
	for (int i = 0; i < names.size(); i++) { assigneeIds.push_back(i + 1); }
	for (int i = 0; i < 5; i++) { assigneeIds.push_back(0); } // just to have a chance to have no assignee
	std::vector<TaskStatus> statuses = {TaskStatus::OPEN, TaskStatus::IN_PROGRESS, TaskStatus::VERIFY, TaskStatus::DONE};
	std::mt19937 rng(static_cast<unsigned>(time(nullptr)));
    std::uniform_int_distribution<size_t> assigneeDist(0, assigneeIds.size() - 1);
    std::uniform_int_distribution<size_t> statusDist(0, statuses.size() - 1);

	for (int i = 0; i < tasks.size(); i++) {
		Task task;
		task.name = tasks.at(i).first;
		task.description = tasks.at(i).second;
		task.status = statuses[statusDist(rng)];
		task.assigneeId = assigneeIds[assigneeDist(rng)];
		task.lastUpdated = 0;
		task.archived = false;

		dataCache.CreateTask(task);
	}
}

void App::CreateProject() {
	dataCache.Clear();
	uiContext = {};
	uiContext.currentScreen = UIScreen::TASKS_SCREEN;

	dataCache.project.name = "New Project";

	CreateDummyData();
}

void App::OpenProject() {
	std::string path = OpenFileDialog();
	if (path != "") {
		// TODO: load data
	}
}

void App::CloseProject() {
	dataCache.Clear();
	uiContext = {};
	uiContext.currentScreen = UIScreen::HOME_SCREEN;
}

void App::SaveProject() {
	// TODO: Implement
}