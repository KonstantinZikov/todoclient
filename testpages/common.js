$(function () {
    $(".add-button").on("click", function () {
        $newLine = $(".todo-block .template").clone();
        $newLine.removeClass("template");
        $newLine.hide();
        $(".todo-block").prepend($newLine);
        $newLine.show(300);
        bindActions($newLine);
    })

    $(".delete-button").on("click", function () {
        $line = $(this).parent().parent();
        $line.hide(300, function () {
            $line.remove();
        })
    })

    function editClick() {
        $line = $(this).parent().parent();
        var status = $line.attr("data-edit");
        if (status == "true") {
            $line.attr("data-edit", "false");

            var $input = $line.find(".todo-input");
            var name = $input.val();
            var $field = $("<div>");
            $field.text(name);
            $field.addClass("todo-name");
            $input.after($field);
            $input.remove();

            $(this).text("Edit");
        }
        else {
            $line.attr("data-edit", "true");
            var $field = $line.find(".todo-name");
            var name = $field.text();
            var $input = $("<input>");
            $input.val(name);
            $input.addClass("todo-input");
            $field.after($input);
            $field.remove();

            $(this).text("Save");
        }
        
    }

    $(".edit-button").on("click", editClick);

    function bindActions(element){
        element.find(".delete-button").on("click", function () {
            $line = $(this).parent().parent();
            $line.hide(300, function () {
                $line.remove();
            })
        })
        element.find(".edit-button").on("click", editClick);
    }
})