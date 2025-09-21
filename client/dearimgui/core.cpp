#include "core.hpp"

#include "imgui/imgui.h"
#include <string>

#ifdef _WIN32
#include <windows.h>
#include <commdlg.h> // For OPENFILENAME and GetOpenFileName
#endif

using namespace Taskpiea;

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
	return Task();
}

bool DataCache::UpdateTask(const Task& task) {
	return false;
}

ValidationResult DataCache::ValidateTask(const Task& task) {
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
		| ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoSavedSettings;
	ImGuiViewport* viewport = ImGui::GetMainViewport();
	ImGui::SetNextWindowPos(viewport->WorkPos);
	ImGui::SetNextWindowSize(viewport->WorkSize);
	ImGui::Begin("Main", nullptr, flags);
	switch (uiContext.currentScreen) {
	case UIScreen::HOME_SCREEN:
		break;
	case UIScreen::USERS_SCREEN:
		CreateUsersScreen();
		break;
	case UIScreen::TASKS_SCREEN:
		CreateTasksScreen();
		break;
	default:
		break;
	};
	ImGui::End();
}

void App::CreateMainMenu() {
	if (ImGui::BeginMainMenuBar())
	{
		if (ImGui::BeginMenu("File"))
		{
			if (ImGui::MenuItem("Create Project")) { CreateProject(); }
			if (ImGui::MenuItem("Open Project", "Ctrl+O")) { OpenProject(); }
			if (ImGui::MenuItem("Close Project")) { CloseProject(); }
			ImGui::Separator();
			ImGui::MenuItem("Exit", "Alt+F4");
			ImGui::EndMenu();
		}
		if (ImGui::BeginMenu("Project"))
		{
			ImGui::MenuItem("Edit Tasks");
			ImGui::MenuItem("Edit Users");
			ImGui::EndMenu();
		}
		ImGui::EndMainMenuBar();
	}
}

void App::CreateUsersScreen() {
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
				User& user = dataCache.users.at(i);
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

				if (uiContext.usersScreen.editingField == USER_FIELD_NAME && uiContext.usersScreen.editingId == user.id) {
					char buffer[256];
					strncpy_s(buffer, user.name.c_str(), sizeof(buffer));
					ImGui::SetNextItemWidth(200.0f); // Set width to 200 pixels
					if (uiContext.usersScreen.focusFlag) {
						ImGui::SetKeyboardFocusHere();
						uiContext.usersScreen.focusFlag = false;
					}
					auto result = dataCache.ValidateUser(user);
					if (result.isSuccess == false) {
						ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f); // Ensure border is visible
						ImGui::PushStyleColor(ImGuiCol_Border, ImVec4(1.0f, 0.0f, 0.0f, 1.0f)); // Set red border
					}
					if (ImGui::InputText("##Name", buffer, sizeof(buffer), ImGuiInputTextFlags_EnterReturnsTrue)) {
						user.name = buffer;
						dataCache.UpdateUser(user);
						uiContext.usersScreen.editingId = -1;
						uiContext.usersScreen.editingField = -1;
					}
					if (ImGui::IsKeyPressed(ImGuiKey_Escape)) {
						uiContext.usersScreen.editingId = -1;
						uiContext.usersScreen.editingField = -1;
					}
					if (result.isSuccess == false) {
						ImGui::PopStyleColor(); // Reset border color
						ImGui::PopStyleVar(); // Reset border size
					}
				}
				else if (uiContext.usersScreen.editingField == -1) {
					if (ImGui::Selectable(user.name.c_str(), false, ImGuiSelectableFlags_AllowDoubleClick | ImGuiSelectableFlags_AllowItemOverlap)) {
						if (ImGui::IsMouseDoubleClicked(0)) {
							uiContext.usersScreen.editingId = user.id;
							uiContext.usersScreen.editingField = USER_FIELD_NAME;
							uiContext.usersScreen.focusFlag = true;
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
	}

	ImGui::EndTable();
}

void App::CreateTasksScreen() {
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
				ImGui::TextUnformatted("STATUS");
				ImGui::TableNextColumn();
				ImGui::TextUnformatted("ASSIGNEE");
				ImGui::PopID();
			}
		}
	}

	ImGui::EndTable();
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
	uiContext.currentScreen = UIScreen::USERS_SCREEN;

	for (int i = 1; i < 100; i++) {
		User user;
		user.name = "Test User " + std::to_string(i);
		user.lastUpdated = 0;
		user.archived = false;

		dataCache.CreateUser(user);
	}

	for (int i = 1; i < 100; i++) {
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

}

void HomeScreen::CreateUI() {
	//ImGui::Text("HOME SCREEN");
}

void UsersScreen::CreateUI() {
	//ImGui::Text("USERS SCREEN");
}