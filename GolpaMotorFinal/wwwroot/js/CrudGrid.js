// class CrudGrid {

//     static load(id) {
//         const grid = $("#" + id);

//         const component = grid.data("component");

//         grid.load("/Framework/Grid?component=" + component);
//     }

//     static refresh(id) {
//         CrudGrid.load(id);
//     }

// }
//---Dynamic General CRUD Operations---\\

//1---Create---\\

// $(document).on("click", ".btnSaveAdd", function (e) {


//     e.preventDefault();

//     e.stopPropagation();

//     let sendingUrl = $(this).attr("data-action");

//     let refreshUrl = $(this).attr("data-next-action");

//     let targetID = "#" + $(this).attr("data-updating-target-id");

//     let sendingData = $("#frm").serialize();

//     $.post(sendingUrl, sendingData, function (op) {
//         if (op.success.toString() === "true") {

//             CrudGrid.refresh("usersGrid");

//             $.get(refreshUrl, null, function (lst) {
//                 $(targetID).html(lst);
//             })
//         }
//         else {
//             alert(op.message);
//         }
//     });
// });