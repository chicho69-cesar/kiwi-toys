const dropdownCascade = (urlGetStates, urlGetCities) => {
    $("#CountryId").change(function () {
        $("#StateId").empty();
        $("#StateId").append('<option value="0">[Selecciona un Departamento / Estado...]</option>');
        $("#CityId").empty();
        $("#CityId").append('<option value="0">[Selecciona una ciudad...]</option>');

        $.ajax({
            type: 'POST',
            url: urlGetStates,
            dataType: 'json',
            data: { countryId: $("#CountryId").val() },
            success: function (states) {
                $.each(states, function (i, state) {
                    $("#StateId").append('<option value="' + state.id + '">' + state.name + '</option>');
                });
            },
            error: function (ex) {
                alert('Failed to retrieve states.' + ex);
            }
        });

        return false;
    });

    $("#StateId").change(function () {
        $("#CityId").empty();
        $("#CityId").append('<option value="0">[Selecciona una ciudad...]</option>');

        $.ajax({
            type: 'POST',
            url: urlGetCities,
            dataType: 'json',
            data: { stateId: $("#StateId").val() },
            success: function (cities) {
                $.each(cities, function (i, city) {
                    debugger;
                    $("#CityId").append('<option value="' + city.id + '">' + city.name + '</option>');
                });
            },
            error: function (ex) {
                alert('Failed to retrieve cities.' + ex);
            }
        });

        return false;
    });
}