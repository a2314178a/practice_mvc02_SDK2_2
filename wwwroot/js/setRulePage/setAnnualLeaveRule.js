

//#region spLeaveRule

function setOption(){
    var yearSel = $(".template").find("select[name='yearSel']").append(new Option("半年", 0.5));
    var spDaySel = $(".template").find("select[name='daySel']");
    for(let i=1; i<=30; i++){
        //yearSel.append(new Option(`${i}年`, i));
        spDaySel.append(new Option(`${i}天`, i));
    }

    var buffSel = $(".template").find("select[name='buffDaySel']").append(new Option("0天", 0));
    for(let i=1; i<=12; i++){
        buffSel.append(new Option(`${i}個月`, i));
    }
    buffSel.append(new Option("無期限", 1200)); //100year = 1200month
}

function showSpLeaveRule(){
    $('.btnActive').css('pointer-events', "");
    $(".add_spLeave").show();
    getAllSpLeaveRule();
}

function getAllSpLeaveRule(){
    var successFn = function(res){
        refreshSpLeaveRuleList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllSpLeaveRule", null, successFn);
}

function refreshSpLeaveRuleList(res){
    var seniority = [0.5];
    for(let i =1; i<=30;i++){
        seniority.push(i);
    }
    $("#spLeaveRuleList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='spLeaveRow']").clone();
        var yText = value.seniority == 0.5 ? "6個月" : value.seniority + "年";
        seniority = seniority.filter(function(val, key){return val != value.seniority});
        row.find("[name='years']").text(yText);
        row.find("[name='days']").text(value.specialDays + "天");
        var buffTest = (value.buffDays/30) + "個月";
        buffTest = value.buffDays==0? "0天" : value.buffDays==36000 ? "無期限" : buffTest;
        row.find("[name='buffDays']").text(buffTest);
        row.find(".edit_spLeave").attr("onclick",`editSpLeaveRule(this, ${value.id});`);
        row.find(".del_spLeave").attr("onclick",`delSpLeaveRule(${value.id});`);
        $("#spLeaveRuleList").append(row);
    });
    var yearSel = $(".template").find("select[name='yearSel']").empty();
    seniority.forEach((val)=>{
        if(val==0.5){
            yearSel.append(new Option("6個月", val));
        }else{
            yearSel.append(new Option(val+"年", val));
        }
    });
}

function showAddSpLeaveRow(){
    var addSpLeaveRow = $(".template").find("[name='addSpLeaveRow']").clone();
    if(addSpLeaveRow.find("select[name='yearSel']").find("option").length == 0){
        alert("已無可新增的項目");
        return;
    }
    addSpLeaveRow.find("a.update_spLeave").remove();
    addSpLeaveRow.find("a.create_spLeave").attr("onclick", "addUpSpLeaveRule(this);");
    addSpLeaveRow.find("a.cancel_spLeave").attr("onclick", "cancelAddSpLeave(this);");
    $("#annualLeaveDiv").find("a.add_spLeave").hide();
    $('.btnActive').css('pointer-events', "none"); 
    $('#spLeaveRuleList').append(addSpLeaveRow);
}

function addUpSpLeaveRule(thisBtn, ruleID=0){
    var thisRow = $(thisBtn).closest("tr[name='addSpLeaveRow']");
    var seniority = $(thisRow).find("select[name='yearSel']").val();
    var specialDays = $(thisRow).find("select[name='daySel']").val();
    var buffDays = $(thisRow).find("select[name='buffDaySel']").val();
    buffDays*=30;

    var data={
        ID:ruleID, seniority, specialDays, buffDays
    };

    var successFn = ()=>{
        showSpLeaveRule();
    }
    myObj.cudAjaxFn("/SetRule/addUpSpLeaveRule", data, successFn);
}

function cancelAddSpLeave(thisBtn){
    showSpLeaveRule();
}

function editSpLeaveRule(thisBtn, ruleID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='spLeaveRow']").hide();
    var thisYear = thisRow.find("[name='years']").text();
    var thisYearVal = thisYear == "6個月"? 0.5 : thisYear.substring(0, thisYear.length-1);
    var thisSpDay = thisRow.find("[name='days']").text();
    var thisBuffDay = thisRow.find("[name='buffDays']").text();

    var upSpLeaveRow = $(".template").find("[name='addSpLeaveRow']").clone();
    upSpLeaveRow.find("select[name='yearSel']").prepend(new Option(thisYear, thisYearVal));
    $.each(upSpLeaveRow.find("[name='yearSel']").find("option"), function(){
        if($(this).text() == thisYear){
            $(this).prop("selected", true);
        }
    });
    $.each(upSpLeaveRow.find("[name='daySel']").find("option"), function(){
        if($(this).text() == thisSpDay){
            $(this).prop("selected", true);
        }
    });
    $.each(upSpLeaveRow.find("[name='buffDaySel']").find("option"), function(){
        if($(this).text() == thisBuffDay){
            $(this).prop("selected", true);
        }
    });

    upSpLeaveRow.find("a.create_spLeave").remove();
    upSpLeaveRow.find("a.update_spLeave").attr("onclick", `addUpSpLeaveRule(this, ${ruleID})`);
    upSpLeaveRow.find("a.cancel_spLeave").attr("onclick", "cancelAddSpLeave(this)");
    $(thisRow).after(upSpLeaveRow);
}

function delSpLeaveRule(ruleID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showSpLeaveRule();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delSpLeaveRule", {ruleID}, successFn);
}

//#endregion spLeaveRule

