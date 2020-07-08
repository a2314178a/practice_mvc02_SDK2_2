

//#region specialDate

function showSpecialDate(){
    $('.btnActive').css('pointer-events', "");
    $(".add_spDate").show();
    getAllSpecialDate();
}

function getAllSpecialDate(){
    var successFn = function(res){
        refreshSpDateList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllSpecialDate", null, successFn);
}

function refreshSpDateList(res){
    $("#specialDateList").empty();
    res.forEach(function(value){
        var dt = myObj.dateTimeFormat(value.date);
        var row = $(".template").find("[name='specialDateRow']").clone();
        row.find("[name='date']").text(dt.ymdText + " " + dt.worldWeek);
        row.find("[name='needClass']").text(value.departClass);
        row.find("input[name='statusVal']").val(value.status);
        row.find("[name='status']").text((value.status == 1? "休假" : "上班"));
        row.find("[name='note']").text(value.note);
        row.find(".edit_spDate").attr("onclick", `editSpecialDate(this, ${value.id});`);
        row.find(".del_spDate").attr("onclick", `delSpecialDate(${value.id});`);
        $("#specialDateList").append(row);
     });
}

function showAddSpDateRow(){
    $("#specialDateDiv").find("a.add_spDate").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addSpDateRow = $(".template").find("[name='addSpecialDateRow']").clone();
    var dt = myObj.dateTimeFormat();
    addSpDateRow.find("input[type='date']").val(dt.ymdHtml);
    addSpDateRow.find("a.update_spDate").remove();
    addSpDateRow.find("a.create_spDate").attr("onclick", "addUpSpecialDate(this);");
    addSpDateRow.find("a.cancel_spDate").attr("onclick", "celSpecialDate(this);");
    $('#specialDateList').append(addSpDateRow);
}

function celSpecialDate(){
    showSpecialDate();
}

function delSpecialDate(spDateID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showSpecialDate();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delSpecialDate", {spDateID}, successFn);
}

function editSpecialDate(thisBtn, spDateID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='specialDateRow']").hide();
    var thisDate = thisRow.find("[name='date']").text();
    var dateVal = thisDate.split(" ")[0].replace(new RegExp("/", "g"), "-");
    var thisClass = thisRow.find("[name='needClass']").text();
    var thisStatus = thisRow.find("input[name='statusVal']").val();
    var thisNote = thisRow.find("[name='note']").text();

    var updateSpDate = $(".template").find("[name='addSpecialDateRow']").clone();
    updateSpDate.find("input[name='newDate']").val(dateVal);
    updateSpDate.find("select[name='needClassVal']").find(`option[value='${thisClass}']`).prop("selected", true);
    updateSpDate.find("select[name='status']").find(`option[value='${thisStatus}']`).prop("selected", true);
    updateSpDate.find("input[name='newNote']").val(thisNote);

    updateSpDate.find("a.create_spDate").remove();
    updateSpDate.find("a.update_spDate").attr("onclick", `addUpSpecialDate(this, ${spDateID})`);
    updateSpDate.find("a.cancel_spDate").attr("onclick", "celSpecialDate()");

    $(thisRow).after(updateSpDate);
}

function addUpSpecialDate(thisBtn, spDateID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addSpecialDateRow']");
    var date = thisRow.find("[name='newDate']").val();
    var departClass = thisRow.find("select[name='needClassVal']").val();
    var status = thisRow.find("select[name='status']").val();
    var note = thisRow.find("[name='newNote']").val();

    if(date == "" || status ==""){
        alert("日期與狀態欄位不可為空");
        return;
    }
    var data = {
        ID : spDateID,
        date : date,
        departClass : departClass,
        status : status,
        note : note,
    };

    var successFn = function(res){
        showSpecialDate();
    };
    myObj.cudAjaxFn("/SetRule/addUpSpecialTime", data, successFn);
}

//#endregion specialDate

