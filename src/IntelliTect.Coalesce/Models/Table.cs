using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Models
{
    public class Table
    {
        public static Table<T> Create<T>()
        {
            return new Table<T>();
        }

        public List<Column> Columns { get; } = new List<Column>();
        public ClassViewModel ViewModel;

        public bool HasSearch { get; set; } = true;
        public bool HasPaging { get; set; } = true;
        public bool HasLoading { get; set; } = true;
        public bool IsEditable { get; set; } = false;
        public bool HasRowDelete { get; set; } = true;
        public bool HasRowEdit { get; set; } = true;
        public bool HasCreate { get; set; } = true;
    }


    public class Table<T> : Table
    {

        /// <summary>
        /// Creates a table.
        /// </summary>
        /// <returns></returns>
        internal Table() {
            ViewModel = ReflectionRepository.GetClassViewModel<T>();

        }

        /// <summary>
        /// Adds all default columns to the table.
        /// </summary>
        /// <returns></returns>
        public Table<T> AddDefaultColumns()
        {
            foreach (var prop in ViewModel.Properties
                .Where(f=>!f.IsHidden( DataAnnotations.HiddenAttribute.Areas.List))
                .OrderBy(f=>f.EditorOrder))
            {
                Columns.Add(new Column
                {
                    ViewModel = prop,
                    Title = prop.DisplayName
                });
            }
            return this;
        }

        /// <summary>
        /// Adds a column to the table.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="title"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public Table<T> Column<TProperty>(Expression<Func<T, TProperty>> property, string title = null, string format = null)
        {
            Columns.Add(new Column
            {
                ViewModel = ViewModel.PropertyBySelector<T, TProperty>(property),
                Title = title,
                Format = format
            });
            return this;
        }

        /// <summary>
        /// Removes a column. Typically used with AddDefaultColumns.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> RemoveColumn(string name)
        {
            var col = Columns.FirstOrDefault(f => f.ViewModel.Name == name);
            if (col != null) Columns.Remove(col);
            return this;
        }




        /// <summary>
        /// Turns on or off Searching.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> Search(bool value = true)
        {
            this.HasSearch = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Paging.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> Paging(bool value = true)
        {
            this.HasPaging = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Editing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> Editing(bool value = false)
        {
            this.IsEditable = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Editing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> Loading(bool value = true)
        {
            this.HasLoading = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Editing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> RowDelete(bool value = true)
        {
            this.HasRowDelete = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Editing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> RowEdit(bool value = true)
        {
            this.HasRowEdit = value;
            return this;
        }

        /// <summary>
        /// Turns on or off Editing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Table<T> AddCreate(bool value = true)
        {
            this.HasCreate = value;
            return this;
        }



    }


    public class Column
    {
        public PropertyViewModel ViewModel { get; set; }
        public string Title { get; set; } = null;
        public string Format { get; set; } = null;

        public string TitleDisplay
        {
            get
            {
                return Title ?? ViewModel.DisplayName;
            }
        }
    }

}
