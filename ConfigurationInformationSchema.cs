﻿
namespace GtkTest
{
	/// <summary>
	/// Структура таблиць і стовбців бази даних
	/// </summary>
	public class ConfigurationInformationSchema
	{
		/// <summary>
		/// Структура таблиць і стовбців бази даних
		/// </summary>
		public ConfigurationInformationSchema()
		{
			Tables = new Dictionary<string, ConfigurationInformationSchema_Table>();
		}

		/// <summary>
		/// Таблиці
		/// </summary>
		public Dictionary<string, ConfigurationInformationSchema_Table> Tables { get; }

		/// <summary>
		/// Дабавлення інформації в структуру
		/// </summary>
		/// <param name="table">Таблиця</param>
		/// <param name="column">Стовпець</param>
		/// <param name="dataType">Тип даних</param>
		/// <param name="udtName">Тип даних</param>
		public void Append(string table, string column, string dataType, string udtName)
		{
			if (!Tables.ContainsKey(table))
				Tables.Add(table, new ConfigurationInformationSchema_Table(table));

			Tables[table].Columns.Add(column, new ConfigurationInformationSchema_Column(column, dataType, udtName));
		}

		/// <summary>
		/// Добавлення інформації про індекси
		/// </summary>
		/// <param name="table">Таблиця</param>
		/// <param name="index">Індекс</param>
		public void AppendIndex(string table, string index)
		{
			if (!Tables.ContainsKey(table))
				Tables.Add(table, new ConfigurationInformationSchema_Table(table));

			Tables[table].Indexes.Add(index, new ConfigurationInformationSchema_Index(index));
		}
	}

	/// <summary>
	/// Таблиця
	/// </summary>
	public class ConfigurationInformationSchema_Table
	{
		/// <summary>
		/// Таблиця
		/// </summary>
		/// <param name="tableName">Назва таблиці</param>
		public ConfigurationInformationSchema_Table(string tableName)
		{
			TableName = tableName;
			Columns = new Dictionary<string, ConfigurationInformationSchema_Column>();
			Indexes = new Dictionary<string, ConfigurationInformationSchema_Index>();
		}

		/// <summary>
		/// Назва таблиці
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// Стовпці
		/// </summary>
		public Dictionary<string, ConfigurationInformationSchema_Column> Columns { get; }

		/// <summary>
		/// Індекси
		/// </summary>
		public Dictionary<string, ConfigurationInformationSchema_Index> Indexes { get; }
	}

	/// <summary>
	/// Стовпчик
	/// </summary>
	public class ConfigurationInformationSchema_Column
	{
		public ConfigurationInformationSchema_Column()
		{
			ColumnName = "";
			DataType = "";
			UdtName = "";
		}

		/// <summary>
		/// Стовпчик
		/// </summary>
		/// <param name="columnName">Назва стовпця</param>
		/// <param name="dataType">Тип даних</param>
		/// <param name="udtName">Тип даних</param>
		public ConfigurationInformationSchema_Column(string columnName, string dataType, string udtName)
		{
			ColumnName = columnName;
			DataType = dataType;
			UdtName = udtName;
		}

		/// <summary>
		/// Назва стовпця
		/// </summary>
		public string ColumnName { get; set; }

		/// <summary>
		/// Тип даних
		/// </summary>
		public string DataType { get; set; }

		/// <summary>
		/// Тип даних
		/// </summary>
		public string UdtName { get; set; }
	}

	/// <summary>
	/// Індекс
	/// </summary>
	public class ConfigurationInformationSchema_Index
	{
		public ConfigurationInformationSchema_Index()
		{
			IndexName = "";
		}

		/// <summary>
		/// Індекс
		/// </summary>
		/// <param name="indexName"></param>
		public ConfigurationInformationSchema_Index(string indexName)
		{
			IndexName = indexName;
		}

		/// <summary>
		/// Назва індексу
		/// </summary>
		public string IndexName { get; set; }
	}
}