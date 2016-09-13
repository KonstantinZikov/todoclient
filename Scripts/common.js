var server = new AjaxManager();

// loads all values from server
function load() {
    server.get(function (todos) {
        todos.forEach(function (item, index, array) {
            $newLine = $(".todo-block .template-value").clone();
            $newLine.removeClass("template-value");
            $newLine.find(".todo-name").text(item.Name);
            $newLine.attr("data-id", item.ToDoId);
            $newLine.find("input:checkbox").prop("checked", item.IsCompleted);
            $(".todo-block").prepend($newLine);
            bindActions($newLine);
        });
    })
}

// edit-/save-buttin action
function editClick() {
    $this = $(this);
    $line = $this.parent().parent();
    var status = $line.attr("data-edit");

    if (status == "true")
        endEdit($line, $this);
    else
        startEdit($line, $this);
}

// Called, when user want to start edit.
// @$line: line with todo-value
// @$button: edit-button
function startEdit($line, $button) {
    $line.attr("data-edit", "true");
    var $field = $line.find(".todo-name");
    var name = $field.text();
    var $input = $("<input>");
    $input.val(name);
    $input.addClass("todo-input");
    $field.after($input);
    $field.remove();
    $button.text("Save");
}

// Called, when user finished to edit and want to save value.
// Causes updating on server.
// @$line: line with input
// @$button: save-button
function endEdit($line, $button) {
    $line.attr("data-edit", "false");
    var $input = $line.find(".todo-input");
    var name = $input.val();
    var $field = $("<div>");
    $field.text(name);
    $field.addClass("todo-name");
    $input.after($field);
    $input.remove();
    $button.text("Edit");
    var id = +$line.attr("data-id");
    var isCompleted = $line.find(".checkbox").prop("checked");

    var entity = {
        ToDoId: id,
        IsCompleted: isCompleted,
        Name: name
    }

    if (id == -1) {
        server.create(entity, function (index) {
            $line.attr("data-id", index);
        });
    }
    else {
        server.update(entity);
    }
}

// Delete todo action
function deleteClick() {
    $line = $(this).parent().parent();
    var id = $line.attr("data-id");
    if (id != -1) {
        server.delete(id);
    }
    
    $line.hide(300, function () {
        $line.remove();
    });
}

// On click to checkbox
function checked() {
    var $this = $(this);
    $line = $this.parent();
    if ($line.attr("data-id") != "-1") {
        var $input = $line.find(".todo-input");
        if ($input.length != 0) {
            var name = $input.val();
        }
        else {
            name = $line.find(".todo-name").text();
        }

        var id = +$line.attr("data-id");
        var isCompleted = $this.prop("checked");

        var entity = {
            ToDoId: id,
            IsCompleted: isCompleted,
            Name: name
        }
        server.update(entity);
    }    
}

// Search action elements in selected element (it should be .todo-value element)
// and binds actions to them
function bindActions(element) {
    element.find(".delete-button").on("click", deleteClick);
    element.find(".edit-button").on("click", editClick);
    element.find(".checkbox").on("click", checked);
}

// Binds add-new-todo logic to Add button
function bindAddAction() {
    $(".add-button").on("click", function () {
        $newLine = $(".todo-block .template").clone();
        $newLine.removeClass("template");
        $newLine.hide();
        $(".todo-block").prepend($newLine);
        $newLine.show(300);
        bindActions($newLine);
    })
}

$(function () {
    load();
    bindAddAction();
    bindActions($(".todo-value"));
})