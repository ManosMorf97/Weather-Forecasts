const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Sites', {
    Site_Id: {
      autoIncrement: true,
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true
    },
    Site_name: {
      type: DataTypes.STRING(450),
      allowNull: false,
      defaultValue: "(N"
    }
  }, {
    sequelize,
    tableName: 'Sites',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Sites_Site_name",
        unique: true,
        fields: [
          { name: "Site_name" },
        ]
      },
      {
        name: "PK_Sites",
        unique: true,
        fields: [
          { name: "Site_Id" },
        ]
      },
    ]
  });
};
