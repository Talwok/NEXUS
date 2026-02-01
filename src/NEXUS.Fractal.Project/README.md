# Формат файла проекта

Файл проекта это ZIP-архив, содержит:
* Список сущностей (entities)
  * Сущность (ProjectEntity)
    * Идентификатор - Id (Guid)
    * Тип сущности - Type (ProjectEntityType)
      * Карта высот - Heightmap (float[,])
      * График - Chart (float[,])
    * Имя - Name (string)
    * Дата последнего редактирования - LastModified (DateTime)
* Список действий над сущностями (actions)
  * Действие (ProjectEntityDataAction)
    * 
* Папка с подпапками данных сущностей (data)
  * Папка данных сущности (GUID) 
    * Данные сущности (INDEX_GUID)