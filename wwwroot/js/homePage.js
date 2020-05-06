
var myObj = new MyObj();


$(document).ready(function() {  
    
    $("form").on("submit", function(e){ 
        e.preventDefault();
        //$("button[type='submit']").prop("disabled",true);
        var account = $('#account').val();
        var password = $('#password').val();
        if(account=="" || password==""){
            alert("帳號或密碼不可空白");
            return;
        }
 
        var data={
            account: account,
            password: password
        };
        var successFn = function(res){
            if(res==0)
                alert("帳號或密碼有誤");
            else
                window.location.href = "/Home/";  
        };
        myObj.rAjaxFn("post","/Home/login",data,successFn);
    });
    
    //$("button[type='submit']").trigger("click");
});//.ready function

