

$(document).ready()
{

    var tasksurl = "/tasks/search/" + window.location.search;
    $.ajax({
        method: "GET",
        url: tasksurl,
        async: false,
        dataType: "JSON",
        success: function (data) {
            var listhtml = '';
            var currentdate = '';
            var nextdate = ''

            if (data.length == 0) {
                listhtml = "No Results";
            } else {
                listhtml = listhtml + "<ul>";
                for (var i = 0; i < data.length; i++) {
                    nextdate = $.format.date(data[i].TaskDate, "MM/dd/yyyy");
                    if (nextdate != currentdate) {
                        if (i > 0) {
                            listhtml = listhtml + "</ul></li>";
                        }
                        listhtml = listhtml + "<li><b>Date:</b> " + nextdate + "</li>";
                        listhtml = listhtml + "<li><b>Total Time:</b> " + data[i].DailyHours + "</li>";
                        listhtml = listhtml + "<li><b>Notes:</b><ul style='margin-bottom:15px;'>";
                    }

                    listhtml = listhtml + "<li>" + data[i].Hours + " hours: " + data[i].Notes + "</li>";

                    currentdate = nextdate;
                }
                listhtml = listhtml + "</ul></li></ul>";
            }
            $("#TaskList").html(listhtml);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#TaskList").html(xhr.responseText);
        }
    });

}