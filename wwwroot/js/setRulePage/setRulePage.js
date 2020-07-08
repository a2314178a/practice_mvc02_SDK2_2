
var myObj = new MyObj();

$(document).ready(function() {  
    
    init();

    $("ul[group='table']").on("click", "a", function(){
        $(".add_timeRule").show();
        $('.btnActive').css('pointer-events', "");
    });

    $("#timeRuleDiv").on("input", "input[name='newElasticityMin']", function(){
        var val = $(this).val();
        val = val.replace(/[^\d]/g, ""); //把非數字的都替換掉，除了數字
        $(this).val(val);
    });
});//.ready function

function showSetRulePage(page){
    window.location.href = "/SetRule/Index?page="+page;  
}


function init(){

    if($("#timeRuleDiv").length > 0){
        getAllTimeRule();
    }else if($("#groupRuleDiv").length > 0){
        showGroupRule();
    }else if($("#specialDateDiv").length > 0){
        showSpecialDate();
        var successFn = function(res){
            res.forEach(function(value){
                $(".template").find("select[name='needClassVal']").append(new Option(value, value));
            });
        };
        myObj.rAjaxFn("get", "/SetRule/getClassDepart", null, successFn); 
    }else if($("#leaveTimeDiv").length >0){
        showLeaveRule();
    }else if($("#annualLeaveDiv").length >0) {
        showSpLeaveRule();
        setOption();
    }
}

