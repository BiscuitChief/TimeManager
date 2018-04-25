$(document).ready()
{
    //Custom jquery regex validator to handle the password field.
    jQuery.validator.addMethod("passwordregex", function (value, element) {
        return this.optional(element) || /^(?=^.{8,30}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*])(?!.*\s)(?!.*[()_+}{"":;'?/>.<,]).*$/.test(value);
    }, "Please enter a valid password.");
    //Setup the jquery validation
    $("form").validate({
        rules: {
            UserName: {
                required: true,
                maxlength: 30,
                minlength: 5
            },
            Password: {
                required: true,
                maxlength: 30,
                minlength: 8,
                passwordregex: true
            }
        }
    });

    //Register a new user
    $("#btnRegister").click(function () {
        //Make sure the form is valid
        if ($("form").valid()) {
            //Disable the register button so the user can't accidentally click it twice
            $("#btnRegister").prop("disabled", true);
            var username = $("#UserName").val();
            var password = $("#Password").val();
            $.ajax({
                method: "POST",
                url: "/users/",
                data: { UserName: username, Password: password },
                success: function (data) {
                    //If user creation succeeds, hide any errors and log the user into the site
                    HideError();
                    $("#btnLogin").click();
                    $("#btnRegister").prop("disabled", false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    ShowError(xhr.responseText);
                    $("#btnRegister").prop("disabled", false);
                }
            });
        }
    });

    //Login method
    $("#btnLogin").click(function () {
        //Make sure the form is valid
        if ($("form").valid()) {
            var username = $("#UserName").val();
            var password = $("#Password").val();
            var returnurl =
            $.ajax({
                method: "POST",
                url: "/login/",
                data: { UserName: username, Password: password },
                success: function (data) {
                    //redirect to the returned Url if successful
                    window.location = "/TimeManager/";
                },
                error: function (xhr, textStatus, errorThrown) {
                    ShowError(xhr.responseText);
                }
            });
        }
    });
}

function ShowError(message) {
    $("#RegistrationError").show();
    $("#RegistrationError").html(message);
}

function HideError() {
    $("#RegistrationError").hide();
    $("#RegistrationError").html("");
}
