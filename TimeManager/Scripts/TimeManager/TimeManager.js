//Custom jquery regex validator to handle the time fields.  Allows for military time as well as AM/PM times.
jQuery.validator.addMethod("time", function (value, element) {
    return this.optional(element) || /^((0[0-9]|1[0-2]):([0-5][0-9])\s?(am|pm|AM|PM))|(([01][0-9]|2[0-3]):([0-5][0-9]))$/.test(value);
}, "Please enter a valid time.");
//Custom jquery regex validator to handle the date fields.
jQuery.validator.addMethod("dateregex", function (value, element) {
    return this.optional(element) || /^((0?[\d]|1[0-2])\/(0?\d|[0-2]\d|[0-2]\d|3[0-1])\/(\d{4}))$/.test(value);
}, "Please enter a valid date.");

//Setup jquery validation
var validator = $("form").validate({
    rules: {
        TaskNote: {
            required: true,
            maxlength: 500
        },
        TaskDate: {
            required: true,
            date: true,
            dateregex: true
        },
        TaskHours: {
            required: true,
            digits: true,
            maxlength: 2,
            max: 24,
            min: 0
        },
        DailyHours: {
            required: true,
            digits: true,
            maxlength: 2,
            max: 24,
            min: 0
        },
        FilterStartDate: {
            date: true,
            dateregex: true
        },
        FilterEndDate: {
            date: true,
            dateregex: true
        },
        EditUserName: {
            required: true
        }
    }
});

$(document).ready()
{
    LoadCurrentUser(); //Load the current user information

    $("#TaskDate").datepicker();
    $("#FilterStartDate").datepicker();
    $("#FilterEndDate").datepicker();

    //Event setup
    $("#searchResults").on("click", "#TaskRow", SelectTask);
    $("#btnCancelTask").click(ClearTaskForm);
    $("#btnFilterTasks").click(LoadTaskList);
    $("#btnPrintTasks").click(PrintTaskList);
    $("#TaskHours").on('change keyup paste', CheckDailyHours);
    $("#EditUserName").change(UpdateUser);
    $("#EditIsAdmin").change(UpdateUser);
    $("#EditIsManager").change(UpdateUser);
    $("#EditIsActive").change(UpdateUser);
    $("#SelectedUserID").change(LoadSelectedUser);
    $("#btnDeleteUser").click(DeleteUser);
    $("#DailyHours").change(UpdateDailyHours);
    $("#btnAddTask").click(AddTask);
    $("#btnDeleteTask").click(DeleteTask);
    $("#TaskDate").change(TaskDateChanged);
}

function AddTask() {
    if (ValidateTaskEntry()) {
        //disable the add button so the user can't accidentally click it twice
        $("#btnAddTask").prop("disabled", true);

        var recordid = $("#TaskRecordID").val();
        if (recordid == "") { recordid = "0"; } //Set the RecordID to 0 if it's a new Task
        var userid = $("#SelectedUserID").val();
        var notes = $("#TaskNotes").val();
        var hours = $("#TaskHours").val();
        var taskdate = $("#TaskDate").val();

        var url = "/Tasks/" + recordid;
        var url = url + "?userid=" + userid;
        var url = url + "&notes=" + encodeURIComponent(notes);
        var url = url + "&hours=" + hours;
        var url = url + "&taskdate=" + taskdate;

        $.ajax({
            method: "POST",
            url: url,
            success: function (data) {
                //on success enable the buttons, reset the data entry form, and load the Task list
                $("#btnAddTask").prop("disabled", false);
                ClearTaskForm();
                LoadTaskList();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
                $("#btnAddTask").prop("disabled", false);
            }
        });
    }
}

//Clear the data entry form
function ClearTaskForm() {
    $("#TaskRecordID").val("");
    $("#TaskNotes").val("");
    $("#TaskDate").val("");
    $("#TaskHours").val("");
    $("#TaskHoursOriginal").val("");
    $("#DailyHoursOtherTasks").val("");

    $(".SelectedTask").removeClass("SelectedTask");
    $("#btnDeleteTask").hide();
    $("#btnAddTask").val("Add Task");

    $("#DailyHours").removeClass("HoursError");
    $("#DailyHours").removeClass("HoursOkay");

    validator.resetForm();
}

//Delete a task
function DeleteTask() {
    var recordid = $("#TaskRecordID").val();

    $.ajax({
        method: "DELETE",
        url: "/Tasks/" + recordid,
        success: function (data) {
            //on success reset the data entry form and load the Task list
            ClearTaskForm();
            LoadTaskList();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    });
}

//Select a specific task from the grid
function SelectTask() {
    $(".SelectedTask").removeClass("SelectedTask"); //clear other selected grid rows
    $(this).addClass("SelectedTask"); //highlight the selected row

    var recordid = $(this).attr("recordid");
    $.ajax({
        method: "GET",
        url: "/tasks/" + recordid,
        dataType: "JSON",
        success: function (data) {
            $("#btnAddTask").val("Edit Task"); //change the add button text
            //Load the meal data
            $("#TaskRecordID").val(data.RecordID);
            $("#TaskNotes").val(data.Notes);
            $("#TaskHours").val(data.Hours);
            $("#TaskHoursOriginal").val(data.Hours); //track the original Task hours so we can calculate daily overages correctly
            $("#TaskDate").val($.format.date(data.TaskDate, "MM/dd/yyyy"));
            $("#DailyHoursOtherTasks").val(data.DailyHours - data.Hours); //Set the daily hours to the daily total minus the Task we are editing
            $("#btnDeleteTask").show();

            CheckDailyHours();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
            ClearTaskForm();
        }
    });
}

//Load the task data
function LoadTaskList() {
    if (ValidateSearchFilters()) {
        //Format the search URL
        var tasksurl = "/tasks/search/?userid=" + $("#SelectedUserID").val() + "&startdate=" + $("#FilterStartDate").val() + "&enddate=" + $("#FilterEndDate").val()
        $.ajax({
            method: "GET",
            url: tasksurl,
            dataType: "JSON",
            success: function (data) {
                //parse out the date and time and format the accordingly
                for (var i = 0; i < data.length; i++) {
                    data[i].TaskDate = $.format.date(data[i].TaskDate, "MM/dd/yyyy");
                }
                //JSRender used to create the grid
                $("#searchResults").html($("#myTemplate").render(data));

                //If a day in the grid is over the expected daily calories, add the CalorieOverage class to that row
                var expectedhours = $("#DailyHours").val();
                if (expectedhours != "" && $("#DailyHours").valid()) {
                    $("[DailyHours]").each(function () {
                        var dailyhours = $(this).attr("DailyHours");
                        if (parseInt(dailyhours) < parseInt(expectedhours)) {
                            $(this).addClass("HoursError");
                        }
                    });
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
            }
        });
    }
}

function PrintTaskList() {
    var tasksurl = "/timemanager/printtasks/?userid=" + $("#SelectedUserID").val() + "&startdate=" + encodeURIComponent($("#FilterStartDate").val()) + "&enddate=" + encodeURIComponent($("#FilterEndDate").val());
    window.open(tasksurl, "_blank");
}

function TaskDateChanged() {
    //Only run if we have a valid date
    if ($("#TaskDate").val() != "" && $("#TaskDate").valid()) {
        var userid = $("#SelectedUserID").val();
        var Taskdate = $("#TaskDate").val();

        $.ajax({
            method: "GET",
            url: "/users/" + userid + "/DailyHours/?Taskdate=" + Taskdate,
            dataType: "JSON",
            success: function (data) {
                //if we are editing a Task we track the calories for the day not including the Task we are editing
                //This way as the new calorie number changes we get the correct total instead of accidentally adding the calories twice
                var taskstarthours = $("#TaskHoursOriginal").val();
                if (taskstarthours != "") {
                    data = parseInt(data) - parseInt(taskstarthours);
                }
                //Can't have a daily calorie value of less than 0
                if (parseInt(data) < 0) { data = 0; }
                $("#DailyHoursOtherTasks").val(data);

                CheckDailyHours();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
                ClearTaskForm();
            }
        });
    }
}

function UpdateDailyHours() {
    if (ValidateUserSettings()) {
        var userid = $("#SelectedUserID").val();
        var settingvalue = $("#DailyHours").val();

        $.ajax({
            method: "POST",
            url: "/usersettings/" + userid + "/DailyHours?settingvalue=" + settingvalue,
            success: function (data) {
                //on success reload the Task list and check the daily calorie totals
                LoadTaskList();
                CheckDailyHours();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
            }
        });
    }
}

//Check to see if we are under the expected daily hours
function CheckDailyHours() {
    $("#DailyHours").removeClass("HoursError");
    $("#DailyHours").removeClass("HoursOkay");

    var expectedhours = $("#DailyHours").val();
    var taskhours = $("#TaskHours").val();
    var dailyhoursother = $("#DailyHoursOtherTasks").val();
    var dailyhours = 0;
    //Check to see if we even have other task hours for the specified date, if not set other daily hours to zero
    if (dailyhoursother != "") {
        dailyhours = dailyhoursother;
    }
    //Verify that we have a value for the meal calories
    if (taskhours != "" && $("#TaskHours").valid()) {
        dailyhours = parseInt(dailyhours) + parseInt(taskhours);
    }
    //Check to see if we are over the expected daily calories
    if (parseInt(dailyhours) < parseInt(expectedhours)) {
        $("#DailyHours").addClass("HoursError");
    } else if (parseInt(expectedhours) > 0 && $("#TaskDate").val() != "") {
        $("#DailyHours").addClass("HoursOkay");  //only do the green highlight if we have an expected hours value
    }
}

//Load data for the current logged in user
function LoadCurrentUser() {
    $.ajax({
        method: "GET",
        url: "/users/" + $("#CurrentUserID").val(),
        dataType: "JSON",
        success: function (data) {
            $("#WeclomeHeading").html("Welcome " + data.UserName);
            $("#CurrentUserID").val(data.UserID);
            $("#SelectedUserID").append($("<option />").val(data.UserID).text("Self")); //Always add the current user first to the dropdown as "Self"
            //Only show the manager options if the user has the appropriate roles
            if (data.IsManager || data.IsAdmin) {
                $("#ManagerOptions").show();
                LoadUserDropdown();
            } else {
                $("#ManagerOptions").hide();
            }
            //Only admins have access to edit admin access
            $("#EditIsAdmin").prop("disabled", !data.IsAdmin);
            //Load the rest of the form with current user JSON data
            LoadUserData(data);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    });
}

//Load the user selected from the dropdown
function LoadSelectedUser() {
    $.ajax({
        method: "GET",
        url: "/users/" + $("#SelectedUserID").val(),
        dataType: "JSON",
        success: function (data) {
            //on success clear the data entry form and load the user data
            //ClearTaskForm();
            LoadUserData(data);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    });
}

//Update user information
function UpdateUser() {
    if ($("#EditUserName").valid()) {

        $.ajax({
            method: "PUT",
            url: "/users/",
            data: {
                UserID: $("#SelectedUserID").val(),
                UserName: $("#EditUserName").val(),
                IsActive: $("#EditIsActive").prop("checked"),
                IsManager: $("#EditIsManager").prop("checked"),
                IsAdmin: $("#EditIsAdmin").prop("checked")
            },
            success: function (data) {
                //On success update the welcome headings and the username in the dropdown list
                if ($("#SelectedUserID").val() == $("#CurrentUserID").val()) {
                    $("#WeclomeHeading").html("Welcome " + $("#EditUserName").val());  //If editing the current user update the welcome message as well
                } else {
                    $("#SelectedUserID option:selected").text($("#EditUserName").val());
                }
                LoadSelectedUser();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
            }
        });
    }
}

//Delete a user
function DeleteUser() {
    $.ajax({
        method: "DELETE",
        url: "/users/" + $("#SelectedUserID").val(),
        success: function () {
            //On success reset the form back to the current logged in user and remove the deleted user from the dropdown list
            var userid = $("#SelectedUserID").val();
            $("#SelectedUserID option").filter("[value='" + userid + "']").remove();

            $("#SelectedUserID").val($("#CurrentUserID").val());
            LoadSelectedUser();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    });
}

//Populate user data on the page from JSON object
function LoadUserData(data) {
    $("#SelectedUserID").val(data.UserID);
    $("#SelectedUserName").html("Viewing data for: " + data.UserName);

    $("#EditUserName").val(data.UserName);
    $("#EditIsActive").prop("checked", data.IsActive);
    $("#EditIsManager").prop("checked", data.IsManager);
    $("#EditIsAdmin").prop("checked", data.IsAdmin);

    //various security checks
    var canedituser = false;
    if (data.UserID == $("#CurrentUserID").val()) {
        //Users can edit themselves
        canedituser = true;
        $("#btnDeleteUser").hide();  //don't allow users to delete themselves
    } else if (data.IsAdmin == false || $("#IsAdmin").val() == "1") {
        //allow editing if the selected user is not an admin, or if the logged in user is an admin
        canedituser = true;
        $("#btnDeleteUser").show();
    } else {
        //do not allow editing and hide the delete button
        canedituser = false;
        $("#btnDeleteUser").hide();
    }

    //enable/disable the form accordingly
    $("#EditUserName").prop("disabled", !canedituser);
    $("#EditIsActive").prop("disabled", !canedituser);
    $("#EditIsManager").prop("disabled", !canedituser);

    if (data.UserID == $("#CurrentUserID").val() || $("#IsAdmin").val() == "1") {
        $("#TaskForm").show();
        $("#SelectedUserName").show();

        //get the expected daily hours setting for the user
        $.ajax({
            method: "GET",
            url: "/usersettings/" + data.UserID + "/DailyHours",
            success: function (data) {
                $("#DailyHours").val(data);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(xhr.responseText);
            }
        });
        //Load the Task list
        LoadTaskList();
        //Secure the Task form if necessary
        TaskFormSecurity();
    } else {
        $("#TaskForm").hide();
        $("#SelectedUserName").hide();
    }
}

//Load the dropdown list of all users
function LoadUserDropdown() {
    $.ajax({
        method: "GET",
        url: "/users/all",
        dataType: "JSON",
        success: function (data) {
            //loop through results to load the dropdown
            for (var i = 0; i < data.length; i++) {
                //Do not add the current logged in user because they will already be first in the list as "Self"
                if (data[i].UserID != $("#CurrentUserID").val()) {
                    $("#SelectedUserID").append($("<option />").val(data[i].UserID).text(data[i].UserName));
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    });
}

//Secure the meal form
function TaskFormSecurity() {
    //Only admins can edit records for other users
    var hasaccess = false;
    if ($("#SelectedUserID").val() == $("#CurrentUserID").val()
        || $("#IsAdmin").val() == "1") {
        hasaccess = true;
    }
    $("#DailyHours").prop("disabled", !hasaccess);
    $("#TaskNotes").prop("disabled", !hasaccess);
    $("#TaskDate").prop("disabled", !hasaccess);
    $("#TaskHours").prop("disabled", !hasaccess);
    $("#btnAddTask").prop("disabled", !hasaccess);
    $("#btnCancelTask").prop("disabled", !hasaccess);
    $("#btnDeleteTask").prop("disabled", !hasaccess);
}

//Validation method for the user settings
function ValidateUserSettings() {
    return ($("#DailyHours").valid());
}

//validation method for the data entry form
function ValidateTaskEntry() {
    return ($("#TaskRecordID").valid() &
            $("#TaskNotes").valid() &
            $("#TaskDate").valid() &
            $("#TaskHours").valid());
}
//validation method for the search filters
function ValidateSearchFilters() {
    return ($("#FilterStartDate").valid() &
            $("#FilterEndDate").valid());
}
