## Build Notes
- This assumes SDL2_DIR is a variable on your PC to the SDL headers and libs



## v1
- save current edited row item in ui context
- remove 1 field requirement and make entire row become "editable"
- make users table control be a popup
- create tasks table
- main menu item to add ne user and task
- save to csv support (plus auto save)
- open project & close project features

## v2
- switch to cmake and add better 10x support
- switch to rad debugger instead of visual studio
- Sqlite file backend
- binary file backend
- settings file


## Considerations
- TOML file backend
- JSON file backend
- UI audio effects
























struct FieldError {
		std::string Field;
		std::string Message;
	};

	struct OperationResult {
		bool IsSuccess;
		std::string ErrorMessage;

		OperationResult() IsSuccess(false) {}
		OperationResult(bool isSuccess) : IsSuccess(isSuccess) {}
		OperationResult(const std::string errorMessage) IsSuccess(false), ErrorMessage(errorMessage) {}

		static OperationResult Success() { return OperationResult(true); }
		static OperationResult Failure(const std::string errorMessage) { return OperationResult(errorMessage); }
	};

	struct ValidationResult {
		bool IsSuccess;
		std::string ErrorMessage;
		std::vector<FieldError> FieldErrors;

		ValidationResult() IsSuccess(false) {}
		ValidationResult(bool isSuccess) : IsSuccess(isSuccess) {}
		ValidationResult(const std::string errorMessage) IsSuccess(false), ErrorMessage(errorMessage) {}

		static ValidationResult Success() { return ValidationResult(true); }
		static ValidationResult Failure(const std::string errorMessage) { return ValidationResult(errorMessage); }

		void AddError(const std::string field, const std::string message) {
			FieldErrors.push_back({field, message});
			IsSuccess = false;
		}
	}

	// THINGS TO DECIDE
	// - should Operation Result have the Id or a Pointer?
	// - can I hanle "errors" cleaner and not have "create" return a result
	// 		- typically I like result wrappers that what the backend operation can just report an error. (ex. sqlite fails or http post fails)
	// 		- perhaps I just return 
	// - I always struggle with the Validation result seperation. Techincally create will call validate too. So if you want validation error msgs
	//	 you would need to make sure you call validate first and process that. In the UI code. If for some reason your UI validated but then the
	//   backend code does not validate, thats just a rare scenario. Handle it best you can.
	// - typically the backend will need to modify a record before completion. (example is auto Id)
	// 		- this means that the UI would need to re request the object ot get the update properties (if I only do Id)
	//		- alternatively I could take away const and modify the object directly
	//		- because data will live in an in memory cache, the UI is always going to pull from that cache.
	// 		  so it is possible that the UI never needs the returned object. Because its in memory cache will be updated and UI
	// 		  will be updated from that. Seeing as my UI is immediate mode, it is rebuilt everytime.

	OperationResult CreateUser(const User& user);
	OperationResult DeleteUser(unsigned int id);
	User GetUser(unsigned int id);
	OperationResult UpdateUser(const User& user);
	ValidationResult ValidateUser(const User& user);