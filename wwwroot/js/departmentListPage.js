
var myObj = new MyObj();

$(document).ready(function() {  
    showDepartment();

    $("ul[group='table']").on("click", "a", function(){
        $(".add_depart").show();
        $('.btnActive').css('pointer-events', "");
    });
});//.ready function

function getAllDepartment(){
    var successFn = function(res){
        refreshDepartList(res);
    };
    myObj.rAjaxFn("get", "/DepartmentList/getAllDepartPosition", null, successFn);
}

function refreshDepartList(res){
    $("#departmentList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='departmentRow']").clone();
        row.find("[name='department']").text(value.department);
        row.find("[name='position']").text(value.position);
        row.find("[name='principalID']").val(value.accID);
        row.find("[name='principal']").text(value.userName);
        row.find(".edit_depart").attr("onclick", `editDepart(this, ${value.id});`);
        row.find(".del_depart").attr("onclick", `delDepart(${value.id});`);
        $("#departmentList").append(row);
     });
}

function showDepartment(){
    $("ul a[name='employee']").removeClass("active");
    $("ul a[name='department']").addClass("active");
    $('.btnActive').css('pointer-events', "");
    $(".add_depart").show();
    getAllDepartment();
}

function showAddDepartRow(){
    var successFn = function(res){
        $("#departmentDiv").find("a.add_depart").hide();
        $('.btnActive').css('pointer-events', "none"); 
        var addDepartmentRow = $(".template").find("[name='addDepartmentRow']").clone();
        res.forEach(function(value){
            addDepartmentRow.find("[name='newPrincipal']").append(new Option(value.userName, value.id));
        });
        addDepartmentRow.find("a.update_depart").remove();
        addDepartmentRow.find("a.create_depart").attr("onclick", "createDepartment(this);");
        addDepartmentRow.find("a.cancel_depart").attr("onclick", "cancelCreateDepart(this);");
        $('#departmentList').append(addDepartmentRow);
    };
    myObj.rAjaxFn("get", "/DepartmentList/getAllPrincipal", null, successFn); 
}

function createDepartment(thisBtn){
    var thisRow =  $(thisBtn).closest("tr[name='addDepartmentRow']");
    var data = {
        department : thisRow.find("[name='newDepartment']").val(),
        position : thisRow.find("[name='newPosition']").val(),
        principalID : thisRow.find("[name='newPrincipal']").val()
    };
    var successFn = function(res){
        $("ul[group='table']").find("a[name='department']").trigger("click");
    }
    myObj.cudAjaxFn("/DepartmentList/createDepartment", data, successFn);
}

function cancelCreateDepart(thisBtn){
    $("ul[group='table']").find("a[name='department']").trigger("click");
}

function delDepart(departID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            getAllDepartment();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/DepartmentList/delDepartment", {departID}, successFn);
}

function editDepart(thisBtn, departID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='departmentRow']").hide();
    var thisDepartment = thisRow.find("[name='department']").text();
    var thisPosition = thisRow.find("[name='position']").text();
    var thisPrincipalID = thisRow.find("[name='principalID']").val();

    var successFn = function(res){
        var updateDepartRow = $(".template").find("[name='addDepartmentRow']").clone();
        updateDepartRow.find("input[name='newDepartment']").val(thisDepartment);
        updateDepartRow.find("input[name='newPosition']").val(thisPosition);
        res.forEach(function(value){
            updateDepartRow.find("[name='newPrincipal']").append(new Option(value.userName, value.id));
            if(value.id == thisPrincipalID){
                updateDepartRow.find("option[value='"+thisPrincipalID+"']").prop("selected", true);
            }
        });
        updateDepartRow.find("a.create_depart").remove();
        updateDepartRow.find("a.update_depart").attr("onclick", `updateDepart(this, ${departID})`);
        updateDepartRow.find("a.cancel_depart").attr("onclick", "cancelCreateDepart(this)");
        $(thisRow).after(updateDepartRow);
    };
    myObj.rAjaxFn("get", "/DepartmentList/getAllPrincipal", null, successFn); 
}

function updateDepart(thisBtn, departID){
    var thisRow =  $(thisBtn).closest("tr[name='addDepartmentRow']");
    var data = {
        ID : departID,
        department : thisRow.find("[name='newDepartment']").val(),
        position : thisRow.find("[name='newPosition']").val(),
        principalID : thisRow.find("[name='newPrincipal']").val()
    };
    var successFn = function(res){
        $("ul[group='table']").find("a[name='department']").trigger("click");
    }
    myObj.cudAjaxFn("/DepartmentList/updateDepartment", data, successFn);
}
