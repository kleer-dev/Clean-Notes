$('.delete-note-btn').on('click', function(){
    let id = localStorage.getItem('id');
    let url = localStorage.getItem('url2');

    // let input = document.querySelector('.count')
    // input.value = $('.note-item').length;
    
    console.log("fef")

    $.ajax({
        url: '/Note/Delete/' + id,
        type: 'POST',
        success: function (result) {
            $('#target').html(result);
            
            //перейти по ссылке
            window.location.href = "/Home/Index";
            

            input.value = $('.note-item').length;
        }
    });

    console.log(id);
});