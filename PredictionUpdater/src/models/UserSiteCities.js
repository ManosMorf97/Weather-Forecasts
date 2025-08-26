const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('UserSiteCities', {
    Email: {
      type: DataTypes.STRING(450),
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Users',
        key: 'Email'
      }
    },
    City_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'CitySites',
        key: 'Site_Id'
      }
    },
    Site_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'CitySites',
        key: 'Site_Id'
      }
    }
  }, {
    sequelize,
    tableName: 'UserSiteCities',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_UserSiteCities_City_Id_Site_Id",
        fields: [
          { name: "City_Id" },
          { name: "Site_Id" },
        ]
      },
      {
        name: "PK_UserSiteCities",
        unique: true,
        fields: [
          { name: "Email" },
          { name: "City_Id" },
          { name: "Site_Id" },
        ]
      },
    ]
  });
};
