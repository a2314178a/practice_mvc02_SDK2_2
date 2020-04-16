
var myObj = new MyObj();

$(document).ready(function() {  
    showEmployee();
    getFilterOption();

    $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
        getAccountByFilter();
    });

    $(window).bind('beforeunload',function(){
        if(myObj.subWin != null && myObj.subWin.open){
            myObj.subWin.close();
        }
    });
});//.ready function

function getFilterOption(){
    var successFn = function(res){
        res.department.forEach(function(value){
            $("#filterDepart").append(new Option(value, value));
        });
        res.position.forEach(function(value){
            $("#filterPosition").append(new Option(value, value));
        });
    };
    myObj.rAjaxFn("get", "/EmployeeList/getFilterOption", null, successFn);
}

function showEmployee(){
    $('.btnActive').css('pointer-events', "");
    getAccountByFilter();
}

function getAccountByFilter(){
    var filterName = $("#filterName").val();
    var filterDepart = $("#filterDepart").val();
    var filterPosition = $("#filterPosition").val();
    var data ={
        fName: filterName,
        fDepart: filterDepart,
        fPosition: filterPosition
    };
    var successFn = function(res){
        refreshAccList(res);
    };
    myObj.rAjaxFn("get", "/EmployeeList/getThisLvAllAcc", data, successFn);
}

function refreshAccList(res){
    $("#accountList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='accountRow']").clone();
        row.find("[name='account']").text(value.account);
        row.find("[name='userName']").find('a').attr("onclick", `showThisPunchLog(${value.id});`).text(value.userName);
        row.find("[name='department']").text(value.department);
        row.find("[name='position']").text(value.position);
        row.find(".edit_user").attr("onclick", `showAddAccWindow(${value.id});`);
        row.find(".del_user").attr("onclick", `delEmployee(${value.id});`);
        $("#accountList").append(row);
    });
}

function delEmployee(employeeID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showEmployee();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/EmployeeList/delEmployee", {employeeID: employeeID}, successFn);
}

function showThisPunchLog(employeeID){
    window.location.href = "/PunchCard/Index?page=log&target="+employeeID;
}

function showAddAccWindow(employeeID=0){
    var successFn = function(res){
        if(res == 1){
            $('.btnActive').css('pointer-events', "none");  
            var closeFn = showEmployee;         
            myObj.openSubWindow(1100, 800, "/EmployeeList/showAddForm?ID=" + employeeID, closeFn);
        }
    };
    myObj.rAjaxFn("get", "/EmployeeList/chkLoginStatus", null, successFn);
}
