//document$.subscribe(function () {

document.addEventListener("DOMContentLoaded", function () {
  const markers = document.querySelectorAll("p.enhance-tutorial-table");
  markers.forEach(marker => {
    let sibling = marker.nextElementSibling;
    while (sibling && !sibling.classList.contains("md-typeset__scrollwrap")) {
      sibling = sibling.nextElementSibling;
    }
    if (sibling) {
      // Find the table inside the wrappers
      const table = sibling.querySelector("table");
      if (table) {
        if (!table.classList.contains('data-table-initialized') && !table.classList.contains('no-datatables')) {
          $(table).DataTable({
            layout: {
              topStart: 'pageLength',
              topEnd: 'search',
              top2Start: {
                buttons: [
                  {
                    extend: 'copy',
                    text: '<i class="fa-regular fa-copy"></i>', // HTML for the icon and text
                  },
                  {
                    extend: 'excel',
                    text: '<i class="fa-regular fa-file-excel"></i>', // HTML for the icon and text
                  },
                  {
                    extend: 'pdf',
                    text: '<i class="fa-regular fa-file-pdf"></i>', // HTML for the icon and text
                  },
                  {
                    extend: 'print',
                    text: '<i class="fa-solid fa-print"></i>', // HTML for the icon and text
                  },
                  {
                    extend: 'colvis',
                    text: '<i class="fa-solid fa-table-columns"></i>', // HTML for the icon and text
                  },
                  //{
                  //      extend: 'searchPanes',
                  //      text: '<i class="fa-solid fa-magnifying-glass"></i>', // HTML for the icon and text
                  //}
                ]
              },
              bottomStart: 'info',
              bottomEnd: 'paging',
            },
            pageLength: 25,
            order: [[0, 'asc']],
            rowGroup: {
              dataSrc: 0
            },
            columnDefs: [
              { visible: false, targets: 0 },
              { searchPanes: { show: false }, targets: [3] }
            ],
            responsive: true,
            initComplete: function () {
                  this.api()
                      .columns()
                      .every(function (colIdx) {
                          // Only add filter for column index 2 (third column)
                          if (colIdx !== 2) return;

                          let column = this;

                          // Create checkbox
                          let label = document.createElement('label');
                          label.style.marginRight = '1em';
                          let checkbox = document.createElement('input');
                          checkbox.type = 'checkbox';
                          checkbox.checked = false; // Unchecked by default
                          checkbox.defaultChecked = false;
                          label.appendChild(checkbox);
                          label.appendChild(document.createTextNode(' Show "How To" guides'));

                          // Custom filter function
                          function filterHowTo(settings, data, dataIndex) {
                              // Only apply to this table
                              if (settings.nTable !== column.table().node()) return true;
                              // If checked, show all rows
                              if (checkbox.checked) {
                                  return true;
                              }
                              // If unchecked, hide rows where column value is "How To"
                              return data[colIdx] !== "How To";
                          }

                          // Register filter
                          $.fn.dataTable.ext.search.push(filterHowTo);

                          // Checkbox event
                          checkbox.addEventListener('change', function () {
                              column.table().draw();
                          });

                          // Insert checkbox above the table
                          let tableNode = column.table().node();
                          let wrapper = tableNode.parentNode.querySelector('.datatable-filters');
                          if (!wrapper) {
                              wrapper = document.createElement('div');
                              wrapper.className = 'datatable-filters';
                              tableNode.parentNode.insertBefore(wrapper, tableNode);
                          }
                          wrapper.appendChild(label);

                          // Force initial filter application so "How To" rows are hidden on first load
                          column.table().draw();

                          // Remove filter when table is destroyed to avoid memory leaks
                          $(tableNode).on('destroy.dt', function () {
                              $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(f => f !== filterHowTo);
                          });
                      });
              }
          }); // Initialize DataTables on the table

          table.classList.add('data-table-initialized'); // Add a class to prevent re-initialization
          table.classList.add("js-enhanced");
        }
      }
    }
  });

  const markersUserGuide = document.querySelectorAll("p.enhance-userguide-table");
  markersUserGuide.forEach(marker => {
    let sibling = marker.nextElementSibling;
    while (sibling && !sibling.classList.contains("md-typeset__scrollwrap")) {
      sibling = sibling.nextElementSibling;
    }
    if (sibling) {
      // Find the table inside the wrappers
      const table = sibling.querySelector("table");
      if (table) {
        if (!table.classList.contains('data-table-initialized') && !table.classList.contains('no-datatables')) {
          $(table).DataTable({
            layout: {
              topStart: 'pageLength',
              topEnd: 'search',
              bottomStart: 'info',
              bottomEnd: 'paging',
            },
            pageLength: 25,
            order: [[0, 'asc']],
            responsive: true
          }); // Initialize DataTables on the table

          table.classList.add('data-table-initialized'); // Add a class to prevent re-initialization
          table.classList.add("js-enhanced");
        }
      }
    }
  });

  var tables = document.querySelectorAll("article table:not([class])");
  tables.forEach(function (table) {
    new Tablesort(table)
  });
})
