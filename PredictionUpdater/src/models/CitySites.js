const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('CitySites', {
    City_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Cities',
        key: 'City_Id'
      }
    },
    Site_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Sites',
        key: 'Site_Id'
      }
    }
  }, {
    sequelize,
    tableName: 'CitySites',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_CitySites_Site_Id",
        fields: [
          { name: "Site_Id" },
        ]
      },
      {
        name: "PK_CitySites",
        unique: true,
        fields: [
          { name: "City_Id" },
          { name: "Site_Id" },
        ]
      },
    ]
  });
};
