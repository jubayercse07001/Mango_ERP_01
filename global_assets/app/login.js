

$(document).ready(function () {
    //function will be called on button click having id btnsave
    $("#btnSubmit").click(function () {
        var vUrl = @Url.Action("DoLogin", "Auth");
        alert(vUrl);
        $.ajax(
            {

                type: "POST", //HTTP POST Method  
                url: , // Controller/View   
                data: { //Passing data  
                    Name: $("#txtName").val(), //Reading text box values using Jquery   
                    City: $("#txtAddress").val(),
                    Address: $("#txtcity").val()
                },
                success: function (data) {
                    return data;
                }

            });


    });
});
