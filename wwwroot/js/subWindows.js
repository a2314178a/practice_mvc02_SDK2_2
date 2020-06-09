
var myObj = new MyObj();

$(document).ready(function() {  

    init();

    $("select[name='department']").on("change", function(){
        $("select[name='position']").empty().append(new Option("請選擇", ""));
        var departSel = $("select[name='department']").val();
        if(departSel == ""){  
            return;
        }
        setAllPosition(departSel);
    });

    $("select[name='position']").on("click", function(){
        var departSel = $("select[name='department']").val();
        if(departSel == ""){
            alert("請先選擇部門");
            return;
        }
    });

    $("select[name='sndManager']").on("change", function(){
        if($(this).val() =="0"){
            return;
        }
        var sel = [{
            id: parseInt($(this).val()),
            userName: $(this).find("option:selected").text()
        }];
        setThisAllManager(sel);
        $(this).find("option[value='0']").prop("selected", true);
    });

});//.ready function

function init(){
    var dt = myObj.dateTimeFormat();
    $("input[name='startWorkDate']").val(dt.ymdHtml)
    getSelOption();
}

function getSelOption(){
    var successFn = function(res){
        setDepartOption(res.departOption);
        setTimeOption(res.timeOption);
        setGroupOption(res.groupOption);
        setPrincipal(res.employeeOption);
        setAgent(res.employeeOption);
        if($("input[name='updateEmployeeBtn']").length > 0){
            var editID = $("input[name='updateEmployeeBtn']").attr("data-id");
            getAccountDetail(editID);
        }
    };
    myObj.rAjaxFn("get", "/EmployeeList/getSelOption", null, successFn);
}

function setDepartOption(res){
    myObj.departPosition = res;
    var departSelList = $("select[name='department']");
    if(($("#ruleVal").val() & 0x0008) > 0){
        var departName= "";
        res.forEach(function(value){
            if(departName != value.department){
                departName = value.department;
                departSelList.append(new Option(value.department, value.department));
            }
        });
    }
    else{
        departSelList.append(new Option(res[0].department, res[0].department));
        setAllPosition(res[0].department);
    }
}

function setPrincipal(res){
    var snd = $("select[name='sndManager']");
    res.forEach(function(value){
        if(value.accLV >1){
            snd.append(new Option(value.userName, value.id));
        }
    });
}

function setAllPosition(departSel){
    $("select[name='position']").empty().append(new Option("請選擇", ""));
    var positionSelList = $("select[name='position']");
    myObj.departPosition.forEach(function(value){
        if(value.department == departSel)
            positionSelList.append(new Option(value.position, value.id));
    });
}

function setTimeOption(res){
    var timeRule = $("select[name='timeRule']");
    res.forEach(function(value){
        myObj.workTimeSpanToTime(value);
        var text = value.name + " - 上班時間 : " + value.startTime + " ~ " + value.endTime;
        timeRule.append(new Option(text, value.id));
    });
}

function setGroupOption(res){
    var groupSelList = $("select[name='actAuthority']");
    res.forEach(function(value){
        groupSelList.append(new Option(value.groupName, value.id));
    }); 
}

function setAgent(res){
    var agent = $("select[name='agent']");
    res.forEach(function(value){
        agent.append(new Option(value.userName, value.id));
    });
}

function setThisAllManager(res){
    if(myObj.thisManager == undefined){
        myObj.thisManager = [];
    }
    res.forEach(function(value){
        if(myObj.thisManager.includes(value.id)){
            return;
        }
        myObj.thisManager.push(value.id);
        var sel = $(`<div id='selManagerID_${value.id}'></div>`);
        var btn = $(`<input type='button' value='x' onclick='delThisSel(${value.id})'>`);
        sel.append(value.userName).append(btn);
        $("#thisManagerDiv").append(sel);
    });
}

function getAccountDetail(employeeID){
    var getAccInfoSuccessFn = function(res){
        var accInfo = res.detail;
        $("input[name='account']").val(accInfo.account);
        $("input[name='userName']").val(accInfo.userName);
        $("select[name='sex']").find(`option[value='${accInfo.sex}']`).prop("selected", "selected");
        $("input[name='humanID']").val(accInfo.humanID);
        $("input[name='birthday']").val(((accInfo.birthday).split("T"))[0]);
        $("input[name='startWorkDate']").val(((accInfo.startWorkDate).split("T"))[0]);
        $("select[name='department']").find(`option[value='${accInfo.department}']`).prop("selected", "selected");
        setAllPosition(accInfo.department);
        $("select[name='position']").find(`option[value='${accInfo.departmentID}']`).prop("selected", true);
        $("select[name='timeRule']").find(`option[value='${accInfo.timeRuleID}']`).prop("selected", true);
        $("select[name='actAuthority']").find(`option[value='${accInfo.groupID}']`).prop("selected", true);
        $("select[name='accLV']").find(`option[value='${accInfo.accLV}']`).prop("selected", true);
        $("select[name='agent']").find(`option[value='${accInfo.myAgentID}']`).prop("selected", true);
        $("input[name='agentEnable']").prop("checked", accInfo.agentEnable);
        myObj.oldManager = (res.manager).map((val)=>{return val.id});
        setThisAllManager(res.manager);
    };
    myObj.rAjaxFn("get", "/EmployeeList/getAccountDetail", {employeeID}, getAccInfoSuccessFn);
}

function delThisSel(thisID){
    if(myObj.thisManager.indexOf(thisID) > -1){
        myObj.thisManager.splice(myObj.thisManager.indexOf(thisID), 1);
    }
    $(`#selManagerID_${thisID}`).remove();
}

function closeSubWin(){
    window.close();
}

function createEmployee(){
    myObj.dataCheck("add","employee");
    if(myObj.errorCode>0){
        myObj.callAlert(myObj.errorCode);
        return;
    }
    var data = {
        account: $("input[name='account']").val(),
        password: $("input[name='password']").val(),
        userName: $("input[name='userName']").val(),
        accLV : $("select[name='accLV']").val(),
        departmentID: $("select[name='position']").val(),
        timeRuleID: $("select[name='timeRule']").val(),
        groupID: $("select[name='actAuthority']").val(),
    };
    var data2 = {
        startWorkDate: $("input[name='startWorkDate']").val(),
    };
    var successFn = function(res){
        if(res== 1){
            window.close();
        }else if(res==0){
            alert("fail");
        }else if(res==-1){
            alert("該帳號已存在");
        }
    };
    myObj.cudAjaxFn("/EmployeeList/createEmployee", {newEmployee:data, employeeDetail:data2}, successFn);
}

function addUpdateEmployee(action, employeeID=0){
    myObj.dataCheck(action, "employee");
    if(myObj.errorCode>0){
        myObj.callAlert(myObj.errorCode);
        return;
    }
    var data = {
        ID: employeeID,
        account: $("input[name='account']").val(),
        password: $("input[name='password']").val(),
        userName: $("input[name='userName']").val(),
        accLV : $("select[name='accLV']").val(),
        departmentID: $("select[name='position']").val(),
        timeRuleID: $("select[name='timeRule']").val(),
        groupID: $("select[name='actAuthority']").val(),
    };
    var data2 = {
        sex: $("select[name='sex']").val(),
        birthday: $("input[name='birthday']").val(),
        humanID: $("input[name='humanID']").val(),
        myAgentID: $("select[name='agent']").val(),
        agentEnable: $("input[name='agentEnable']").prop("checked"),
        startWorkDate: $("input[name='startWorkDate']").val(),
    };

    //判斷負責人是否有變化 -1:no -2:yes
    if(employeeID >0){
        var equal = myObj.oldManager.sort((a,b)=>{return a-b;}).toString() == myObj.thisManager.sort((a,b)=>{return a-b;}).toString() ? -1 : -2;
        myObj.thisManager.unshift(equal);
    }else{     
        myObj.thisManager = myObj.thisManager == undefined? [] : myObj.thisManager;
        myObj.thisManager.unshift(-2);
    }
        
    var sendData = {
        accData: data, 
        employeeDetail: data2, 
        thisManager: myObj.thisManager,
        action: action
    };

    var successFn = function(res){
        if(res== 1){
            window.close();
        }else if(res==0){
            alert("fail");
        }else if(res==-1){
            alert("該帳號已存在");
        }
    };
    myObj.cudAjaxFn("/EmployeeList/addUpdateEmployee", sendData, successFn);
}











