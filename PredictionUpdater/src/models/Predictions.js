const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Predictions', {
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
    },
    Timeslot_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Timeslots',
        key: 'Timeslot_Id'
      }
    },
    Weather: {
      type: DataTypes.TEXT,
      allowNull: true
    },
    Danger: {
      type: DataTypes.BOOLEAN,
      allowNull: false
    }
  }, {
    sequelize,
    tableName: 'Predictions',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Predictions_Timeslot_Id",
        fields: [
          { name: "Timeslot_Id" },
        ]
      },
      {
        name: "PK_Predictions",
        unique: true,
        fields: [
          { name: "City_Id" },
          { name: "Site_Id" },
          { name: "Timeslot_Id" },
        ]
      },
    ]
  });
};
