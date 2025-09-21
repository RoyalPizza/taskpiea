#include "core.hpp"

#include "imgui/imgui.h"
#include <string>

#ifdef _WIN32
#include <windows.h>
#include <commdlg.h> // For OPENFILENAME and GetOpenFileName
#endif

using namespace Taskpiea;

static const int TASK_STATUS_COUNT = 4;
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
	if (uiContext.usersControl.visible) {
		CreateUsersControl();
	}
	if (uiContext.aboutControl.visible) {
		CreateAboutControl();
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
			if (ImGui::MenuItem("Edit Users", nullptr, false, projectIsOpen)) { uiContext.usersControl.Show(); }
			ImGui::EndMenu();
		}
		if (ImGui::BeginMenu("Help")) {
			if (ImGui::MenuItem("About")) { uiContext.aboutControl.Show(); }
			ImGui::EndMenu();
		}
		ImGui::EndMainMenuBar();
	}
}

void App::CreateUsersControl() {
	ImGui::Begin("UsersControl", &uiContext.usersControl.visible);
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
				User& user = (dataCache.users.at(i).id == uiContext.usersControl.editingUser.id)
					? uiContext.usersControl.editingUser
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

				if (uiContext.usersControl.editingUser.id == user.id) {
					char buffer[256];
					strncpy_s(buffer, user.name.c_str(), sizeof(buffer));
					ImGui::SetNextItemWidth(200.0f); // Set width to 200 pixels
					if (uiContext.usersControl.focusFlag) {
						ImGui::SetKeyboardFocusHere();
						uiContext.usersControl.focusFlag = false;
					}
					auto result = dataCache.ValidateUser(user);
					if (result.isSuccess == false) {
						ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f); // Ensure border is visible
						ImGui::PushStyleColor(ImGuiCol_Border, ImVec4(1.0f, 0.0f, 0.0f, 1.0f)); // Set red border
					}
					if (ImGui::InputText("##Name", buffer, sizeof(buffer), ImGuiInputTextFlags_EnterReturnsTrue)) {
						user.name = buffer;
						dataCache.UpdateUser(user);
						uiContext.usersControl.editingUser = User();
					} else {
						user.name = buffer;
					}
					if (ImGui::IsKeyPressed(ImGuiKey_Escape)) {
						uiContext.usersControl.editingUser = User();
					}
					if (result.isSuccess == false) {
						ImGui::PopStyleColor(); // Reset border color
						ImGui::PopStyleVar(); // Reset border size
					}
				}
				else if (uiContext.usersControl.editingUser.id == 0) {
					if (ImGui::Selectable(user.name.c_str(), false, ImGuiSelectableFlags_AllowDoubleClick | ImGuiSelectableFlags_AllowItemOverlap)) {
						if (ImGui::IsMouseDoubleClicked(0)) {
							uiContext.usersControl.editingUser = User(user);
							uiContext.usersControl.focusFlag = true;
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
		ImGui::TableSetupColumn("Name", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_NAME);
		ImGui::TableSetupColumn("Description", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_DESCRPTION);
		ImGui::TableSetupColumn("Status", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_STATUS);
		ImGui::TableSetupColumn("Assignee", ImGuiTableColumnFlags_WidthFixed, 0.0f, TASK_FIELD_ASSIGNEE);
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
				ImGui::Text("%04d", task.id);
				ImGui::TableNextColumn();
				ImGui::TextUnformatted(task.name.c_str());
				ImGui::TableNextColumn();
				ImGui::TextUnformatted(task.description.c_str());
				ImGui::TableNextColumn();

				// status requires no verification, always allow it to change
				const char* comboPreviewValue = GetTaskStatusName(task.status);
				ImGui::SetNextItemWidth(120.0f);
				ImGui::PushStyleColor(ImGuiCol_Text, GetTaskStatusColor(task.status));
				ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0.0f, 0.0f, 0.0f, 0.0f));
				if (ImGui::BeginCombo("", comboPreviewValue, ImGuiComboFlags_HeightSmall))
				{
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

				ImGui::TextUnformatted("ASSIGNEE");
				ImGui::PopID();
			}
		}
	}

	ImGui::EndTable();
}

void App::CreateAboutControl() {
    ImGui::Begin("AboutControl", &uiContext.aboutControl.visible);

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

void App::CreateProject() {
	dataCache.Clear();
	uiContext = {};
	uiContext.currentScreen = UIScreen::TASKS_SCREEN;

	dataCache.project.name = "New Project";

	for (int i = 1; i < 1000; i++) {
		User user;
		user.name = "Test User " + std::to_string(i);
		user.lastUpdated = 0;
		user.archived = false;

		dataCache.CreateUser(user);
	}

	for (int i = 1; i < 1000; i++) {
		Task task;
		task.name = "Test Task " + std::to_string(i);
		task.description = "Some task description.";
		task.status = TaskStatus::OPEN;
		task.assigneeId = 0;
		task.lastUpdated = 0;
		task.archived = false;

		dataCache.CreateTask(task);
	}
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