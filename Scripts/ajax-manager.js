function AjaxManager() {
    // Send post request with creating entity to server and calls callback if success.
    // @entity: todo entity for creating.
    // @callback: function that calls if success. Should have one param - id of created entity.
    this.create = function (entity, callbalck) {
        $.ajax({
            url: "/api/todos",
            type: "POST",
            contentType: 'application/json',
            data: JSON.stringify(entity),
            success: callbalck
        });
    }

    // Send put request with updating entity to server.
    // @entity: todo entity for updating.
    this.update = function (entity) {
        $.ajax({
            url: "/api/todos",
            type: "PUT",
            contentType: 'application/json',
            data: JSON.stringify(entity)
        });
    }

    // Gets all values from server and calls callback if success.
    // @callback: function that calls if success.
    // Should have one param - collection of 'todo' entities.
    this.get = function (callback) {
        $.ajax({
            url: "/api/todos",
            type: "GET",
            contentType: 'application/json',
            success: callback
        });
    }

    // Deletes entity with selected id from server.
    // @id: id of deleting entity.
    this.delete = function (id) {
        $.ajax({
            url: "/api/todos/" + id,
            type: "DELETE"
        });
    }

    // Try to synchronize server to service, returns todos and calls callback if success.
    // @callback: function that calls if success.
    // Should have one param - collection of 'todo' entities.
    this.sync = function (callback) {
        $.ajax({
            url: "/api/sync",
            type: "GET",
            contentType: 'application/json',
            success: callback
        });
    }

    this.waitForSync = function (callback) {
        $.ajax({
            url: "/api/sync",
            type: "POST",
            contentType: 'application/json',
            success: callback
        });
    }
}