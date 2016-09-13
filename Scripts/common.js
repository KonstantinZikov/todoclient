$(function () {
    $(".add-button").on("click", function () {
        $newLine = $(".todo-block .template").clone();
        $newLine.removeClass("template");
        $newLine.hide();
        $(".todo-block").prepend($newLine);
        $newLine.show(300);
        bindActions($newLine);
    })

    function load() {
        $.ajax(
        {
            url: "/api/todos",
            type: "GET",
            contentType: 'application/json',
            success: function (todos) {
                todos.forEach(function (item, index, array) {
                    $newLine = $(".todo-block .template-value").clone();
                    $newLine.removeClass("template-value");
                    $newLine.find(".todo-name").text(item.Name);
                    $newLine.attr("data-id", item.ToDoId);
                    $newLine.find("input:checkbox").prop("checked", item.IsCompleted);                  
                    $(".todo-block").prepend($newLine);
                    bindActions($newLine);
                });
            }
        });
    }

    load();

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

            var id = +$line.attr("data-id");
            var isCompleted = $line.find(".checkbox").prop("checked");
            if (id == -1) {
                var method = "POST";
            }
            else {
                method = "PUT"
            }

            $.ajax(
            {
                url: "/api/todos",
                type: method,
                contentType: 'application/json',
                data: JSON.stringify({
                    ToDoId: id,
                    IsCompleted: isCompleted,
                    Name: name
                }),
                success: function (index) {
                    if (method == "POST") {
                        $line.attr("data-id", index);
                    }
                }
            });
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

    function deleteClick() {
        $line = $(this).parent().parent();
        var id = $line.attr("data-id");
        $.ajax({
            url: "/api/todos/" + id,
            type: "DELETE"
        });

        $line.hide(300, function () {
            $line.remove();
        });
    }

    function bindActions(element){
        element.find(".delete-button").on("click", deleteClick);
        element.find(".edit-button").on("click", editClick);
    }

    bindActions($(".todo-value"));
})